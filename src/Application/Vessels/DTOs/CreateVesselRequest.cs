using System.Text.Json.Serialization;

namespace ShipSharp.Application.Vessels.DTOs;

public class CreateVesselRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("imo_number")]
    public string IMONumber { get; set; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = string.Empty;

    [JsonPropertyName("capacity")]
    public decimal Capacity { get; set; }
}
