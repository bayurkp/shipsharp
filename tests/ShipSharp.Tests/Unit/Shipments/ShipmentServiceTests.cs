using FluentAssertions;
using FluentValidation;
using Moq;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Customers.DTOs;
using ShipSharp.Application.Ports.DTOs;
using ShipSharp.Application.Shipments;
using ShipSharp.Application.Shipments.DTOs;
using ShipSharp.Application.Shipments.Validators;
using ShipSharp.Application.Vessels.DTOs;
using ShipSharp.Domain.Customers;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Shipments;
using ShipSharp.Domain.Vessels;
using Xunit;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Tests.Unit.Shipments;

public class ShipmentServiceTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IPortRepository> _portRepoMock = new();
    private readonly Mock<IVesselRepository> _vesselRepoMock = new();
    private readonly CreateShipmentRequestValidator _createValidator = new();
    private readonly UpdateShipmentRequestValidator _updateValidator = new();
    private readonly ShipmentService _sut;

    public ShipmentServiceTests()
    {
        _sut = new ShipmentService(
            _shipmentRepoMock.Object,
            _customerRepoMock.Object,
            _portRepoMock.Object,
            _vesselRepoMock.Object,
            _createValidator,
            _updateValidator);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateShipmentWithTrackingNumberAndBookedStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var originPortId = Guid.NewGuid();
        var destPortId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        var request = new CreateShipmentRequest
        {
            CustomerId = customerId,
            OriginPortId = originPortId,
            DestinationPortId = destPortId,
            VesselId = vesselId,
            EstimatedDeparture = DateTime.UtcNow.AddDays(1),
            EstimatedArrival = DateTime.UtcNow.AddDays(5),
            Notes = "Test cargo"
        };

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, Name = "Acme Corp", Email = "acme@test.com", Phone = "123", Address = "Street" });

        _portRepoMock.Setup(r => r.GetByIdAsync(originPortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Port { Id = originPortId, Name = "Surabaya", Code = "SUB", Country = "ID" });

        _portRepoMock.Setup(r => r.GetByIdAsync(destPortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Port { Id = destPortId, Name = "Singapore", Code = "SIN", Country = "SG" });

        _vesselRepoMock.Setup(r => r.GetByIdAsync(vesselId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vessel { Id = vesselId, Name = "MV Test", IMONumber = "IMO9123456", Flag = "ID", Capacity = 1000, IsActive = true });

        _shipmentRepoMock.Setup(r => r.GetCountForYearAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.CreateAsync(request, "operator1");

        // Assert
        result.Should().NotBeNull();
        result.TrackingNumber.Should().StartWith($"SHP-{DateTime.UtcNow.Year}");
        result.Status.Should().Be("Booked");
        result.History.Should().HaveCount(1);
        result.History.First().CurrentStatus.Should().Be("Booked");

        _shipmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Shipment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenOriginAndDestinationPortsAreEqual_ShouldThrowValidationException()
    {
        // Arrange
        var portId = Guid.NewGuid();
        var request = new CreateShipmentRequest
        {
            CustomerId = Guid.NewGuid(),
            OriginPortId = portId,
            DestinationPortId = portId,
            VesselId = Guid.NewGuid(),
            EstimatedDeparture = DateTime.UtcNow.AddDays(1),
            EstimatedArrival = DateTime.UtcNow.AddDays(5)
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request, "operator1");
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Message.Contains("Origin and destination ports must be different"));
    }

    [Fact]
    public async Task CreateAsync_WhenVesselIsInactive_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var originPortId = Guid.NewGuid();
        var destPortId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        var request = new CreateShipmentRequest
        {
            CustomerId = customerId,
            OriginPortId = originPortId,
            DestinationPortId = destPortId,
            VesselId = vesselId,
            EstimatedDeparture = DateTime.UtcNow.AddDays(1),
            EstimatedArrival = DateTime.UtcNow.AddDays(5)
        };

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, Name = "Acme", Email = "a@a.com", Phone = "1", Address = "A" });

        _portRepoMock.Setup(r => r.GetByIdAsync(originPortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Port { Id = originPortId, Name = "A", Code = "AAA", Country = "ID" });

        _portRepoMock.Setup(r => r.GetByIdAsync(destPortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Port { Id = destPortId, Name = "B", Code = "BBB", Country = "ID" });

        _vesselRepoMock.Setup(r => r.GetByIdAsync(vesselId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vessel { Id = vesselId, Name = "Inactive Vessel", IMONumber = "IMO1111111", Flag = "ID", Capacity = 100, IsActive = false });

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request, "operator1");
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*inactive vessel*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WithSequentialStep_ShouldAdvanceStatusAndLogHistory()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = shipmentId,
            TrackingNumber = "SHP-20260001",
            Status = ShipmentStatus.Booked
        };

        _shipmentRepoMock.Setup(r => r.GetByIdAsync(shipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        // Act
        var result = await _sut.UpdateStatusAsync(shipmentId, ShipmentStatus.Loading, "operator1");

        // Assert
        result.Status.Should().Be("Loading");
        _shipmentRepoMock.Verify(r => r.AddStatusHistoryAsync(It.Is<ShipmentStatusHistory>(
            h => h.PreviousStatus == ShipmentStatus.Booked && h.CurrentStatus == ShipmentStatus.Loading && h.UpdatedBy == "operator1"),
            It.IsAny<CancellationToken>()), Times.Once);

        _shipmentRepoMock.Verify(r => r.UpdateAsync(shipment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenSkippingSteps_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = shipmentId,
            TrackingNumber = "SHP-20260001",
            Status = ShipmentStatus.Booked
        };

        _shipmentRepoMock.Setup(r => r.GetByIdAsync(shipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        // Act & Assert
        var act = async () => await _sut.UpdateStatusAsync(shipmentId, ShipmentStatus.AtSea, "operator1");
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*strictly sequential*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenShipmentIsDelivered_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = shipmentId,
            TrackingNumber = "SHP-20260001",
            Status = ShipmentStatus.Delivered
        };

        _shipmentRepoMock.Setup(r => r.GetByIdAsync(shipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        // Act & Assert
        var act = async () => await _sut.UpdateStatusAsync(shipmentId, ShipmentStatus.Booked, "operator1");
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*Delivered shipments are immutable*");
    }
}
