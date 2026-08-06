namespace ShipSharp.Application.Auth.DTOs;

public record LogoutRequest(
    string RefreshToken
);
