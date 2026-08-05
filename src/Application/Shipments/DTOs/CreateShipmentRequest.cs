using System.Text.Json.Serialization;

namespace ShipSharp.Application.Shipments.DTOs;

public class CreateShipmentRequest
{
    [JsonPropertyName("customer_id")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("origin_port_id")]
    public Guid OriginPortId { get; set; }

    [JsonPropertyName("destination_port_id")]
    public Guid DestinationPortId { get; set; }

    [JsonPropertyName("vessel_id")]
    public Guid VesselId { get; set; }

    [JsonPropertyName("estimated_departure")]
    public DateTime EstimatedDeparture { get; set; }

    [JsonPropertyName("estimated_arrival")]
    public DateTime EstimatedArrival { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
