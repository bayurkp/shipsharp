using System.Text.Json.Serialization;

namespace ShipSharp.Application.Shipments.DTOs;

public class ShipmentStatusHistoryResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("previous_status")]
    public string? PreviousStatus { get; set; }

    [JsonPropertyName("current_status")]
    public string CurrentStatus { get; set; } = string.Empty;

    [JsonPropertyName("updated_by")]
    public string UpdatedBy { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
