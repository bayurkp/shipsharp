namespace ShipSharp.Application.Auth.DTOs;

public record RegisterRequest(
    string Username,
    string Password,
    string FullName,
    string Role
);
