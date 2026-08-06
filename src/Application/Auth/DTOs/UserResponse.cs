namespace ShipSharp.Application.Auth.DTOs;

public record UserResponse(
    Guid Id,
    string Username,
    string FullName,
    string Role,
    DateTime CreatedAt
);
