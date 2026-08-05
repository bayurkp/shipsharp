using System.Text.Json.Serialization;

namespace ShipSharp.Application.Vessels.DTOs;

public class UpdateVesselRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = string.Empty;

    [JsonPropertyName("capacity")]
    public decimal Capacity { get; set; }
}
