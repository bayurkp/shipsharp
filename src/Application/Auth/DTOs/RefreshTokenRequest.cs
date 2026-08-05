using System.Text.Json.Serialization;

namespace ShipSharp.Application.Auth.DTOs;

public class RefreshTokenRequest
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
