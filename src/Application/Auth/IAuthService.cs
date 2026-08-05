using ShipSharp.Application.Auth.DTOs;

namespace ShipSharp.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
