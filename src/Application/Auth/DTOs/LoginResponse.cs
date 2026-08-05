using System.Text.Json.Serialization;

namespace ShipSharp.Application.Auth.DTOs;

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } = 3600;

    [JsonPropertyName("user")]
    public UserDto User { get; set; } = null!;
}
