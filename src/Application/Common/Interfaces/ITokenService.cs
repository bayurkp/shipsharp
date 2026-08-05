using ShipSharp.Domain.Users;

namespace ShipSharp.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}
