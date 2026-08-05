using System.Text.Json.Serialization;
using ShipSharp.Application.Customers.DTOs;
using ShipSharp.Application.Ports.DTOs;
using ShipSharp.Application.Vessels.DTOs;

namespace ShipSharp.Application.Shipments.DTOs;

public class ShipmentResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("tracking_number")]
    public string TrackingNumber { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public CustomerResponse Customer { get; set; } = null!;

    [JsonPropertyName("origin_port")]
    public PortResponse OriginPort { get; set; } = null!;

    [JsonPropertyName("destination_port")]
    public PortResponse DestinationPort { get; set; } = null!;

    [JsonPropertyName("vessel")]
    public VesselResponse Vessel { get; set; } = null!;

    [JsonPropertyName("estimated_departure")]
    public DateTime EstimatedDeparture { get; set; }

    [JsonPropertyName("estimated_arrival")]
    public DateTime EstimatedArrival { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("history")]
    public List<ShipmentStatusHistoryResponse> History { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
