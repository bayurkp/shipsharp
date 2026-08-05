using System.Text.Json.Serialization;

namespace ShipSharp.Application.Shipments.DTOs;

public class UpdateShipmentRequest
{
    [JsonPropertyName("vessel_id")]
    public Guid VesselId { get; set; }

    [JsonPropertyName("estimated_departure")]
    public DateTime EstimatedDeparture { get; set; }

    [JsonPropertyName("estimated_arrival")]
    public DateTime EstimatedArrival { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
