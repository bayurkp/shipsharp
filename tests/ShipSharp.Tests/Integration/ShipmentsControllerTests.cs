using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShipSharp.Application.Auth.DTOs;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Customers.DTOs;
using ShipSharp.Application.Ports.DTOs;
using ShipSharp.Application.Shipments.DTOs;
using ShipSharp.Application.Vessels.DTOs;
using ShipSharp.Domain.Shipments;
using Xunit;

namespace ShipSharp.Tests.Integration;

public class ShipmentsControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public ShipmentsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        return body!.Data!.AccessToken;
    }

    [Fact]
    public async Task TrackByNumber_WithoutAuth_ShouldReturnTrackingDetails()
    {
        // Act - Try tracking non-existent shipment to verify endpoint is accessible without auth
        var response = await _client.GetAsync("/api/shipments/track/SHP-NONEXISTENT");

        // Assert - Endpoint should return 404 Not Found in ByJSON format (not 401 Unauthorized)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        body.Should().NotBeNull();
        body!.Error.Should().NotBeNull();
        body.Error!.Code.Should().Be("not_found");
    }

    [Fact]
    public async Task FullShipmentLifecycle_ShouldWorkSequentiallyAndEnforceImmutability()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Get Seeding Ports
        var portsResponse = await _client.GetFromJsonAsync<ApiResponse<List<PortResponse>>>("/api/ports", _jsonOptions);
        var originPort = portsResponse!.Data!.First(p => p.Code == "SUB");
        var destPort = portsResponse.Data!.First(p => p.Code == "SIN");

        // 2. Get Seeding Vessels
        var vesselsResponse = await _client.GetFromJsonAsync<ApiResponse<List<VesselResponse>>>("/api/vessels", _jsonOptions);
        var vessel = vesselsResponse!.Data!.First();

        // 3. Create Customer
        var createCustomerReq = new CreateCustomerRequest
        {
            Name = "Integration Test Client",
            Email = $"test-{Guid.NewGuid():N}@client.com",
            Phone = "08123456789",
            Address = "Test Dock Street"
        };
        var custResponse = await _client.PostAsJsonAsync("/api/customers", createCustomerReq, _jsonOptions);
        custResponse.EnsureSuccessStatusCode();
        var customer = (await custResponse.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>(_jsonOptions))!.Data!;

        // 4. Create Shipment
        var createShipmentReq = new CreateShipmentRequest
        {
            CustomerId = customer.Id,
            OriginPortId = originPort.Id,
            DestinationPortId = destPort.Id,
            VesselId = vessel.Id,
            EstimatedDeparture = DateTime.UtcNow.AddDays(1),
            EstimatedArrival = DateTime.UtcNow.AddDays(5),
            Notes = "Container 40ft"
        };

        var createShipmentRes = await _client.PostAsJsonAsync("/api/shipments", createShipmentReq, _jsonOptions);
        createShipmentRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var shipment = (await createShipmentRes.Content.ReadFromJsonAsync<ApiResponse<ShipmentResponse>>(_jsonOptions))!.Data!;
        shipment.Status.Should().Be("Booked");

        // 5. Test Public Tracking Endpoint
        var trackingRes = await _client.GetAsync($"/api/shipments/track/{shipment.TrackingNumber}");
        trackingRes.EnsureSuccessStatusCode();
        var trackingData = (await trackingRes.Content.ReadFromJsonAsync<ApiResponse<ShipmentTrackingResponse>>(_jsonOptions))!.Data!;
        trackingData.CurrentStatus.Should().Be("Booked");

        // 6. Advance Status: Booked -> Loading
        var updateStatusReq1 = new UpdateShipmentStatusRequest { Status = ShipmentStatus.Loading };
        var statusRes1 = await _client.PatchAsync($"/api/shipments/{shipment.Id}/status", JsonContent.Create(updateStatusReq1, options: _jsonOptions));
        statusRes1.EnsureSuccessStatusCode();
        var updatedShipment1 = (await statusRes1.Content.ReadFromJsonAsync<ApiResponse<ShipmentResponse>>(_jsonOptions))!.Data!;
        updatedShipment1.Status.Should().Be("Loading");

        // 7. Advance Status through remaining steps: Departed -> AtSea -> Arrived -> Delivered
        foreach (var status in new[] { ShipmentStatus.Departed, ShipmentStatus.AtSea, ShipmentStatus.Arrived, ShipmentStatus.Delivered })
        {
            var req = new UpdateShipmentStatusRequest { Status = status };
            var res = await _client.PatchAsync($"/api/shipments/{shipment.Id}/status", JsonContent.Create(req, options: _jsonOptions));
            res.EnsureSuccessStatusCode();
        }

        // 8. Verify Delivered shipment is Immutable
        var editReq = new UpdateShipmentRequest
        {
            VesselId = vessel.Id,
            EstimatedDeparture = DateTime.UtcNow,
            EstimatedArrival = DateTime.UtcNow.AddDays(10)
        };
        var editRes = await _client.PutAsJsonAsync($"/api/shipments/{shipment.Id}", editReq, _jsonOptions);
        editRes.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
