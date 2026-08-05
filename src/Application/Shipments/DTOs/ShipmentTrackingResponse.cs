using System.Text.Json.Serialization;

namespace ShipSharp.Application.Shipments.DTOs;

public class ShipmentTrackingResponse
{
    [JsonPropertyName("tracking_number")]
    public string TrackingNumber { get; set; } = string.Empty;

    [JsonPropertyName("current_status")]
    public string CurrentStatus { get; set; } = string.Empty;

    [JsonPropertyName("estimated_arrival")]
    public DateTime EstimatedArrival { get; set; }

    [JsonPropertyName("origin_port")]
    public string OriginPort { get; set; } = string.Empty;

    [JsonPropertyName("destination_port")]
    public string DestinationPort { get; set; } = string.Empty;

    [JsonPropertyName("vessel_name")]
    public string VesselName { get; set; } = string.Empty;

    [JsonPropertyName("history")]
    public List<ShipmentStatusHistoryResponse> History { get; set; } = new();
}
