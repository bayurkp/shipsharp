using FluentValidation;
using ShipSharp.Application.Auth.DTOs;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Interfaces;
using ShipSharp.Application.Common.Models;
using ShipSharp.Domain.Users;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LogoutRequest> _logoutValidator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LogoutRequest> logoutValidator)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _registerValidator = registerValidator;
        _logoutValidator = logoutValidator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            });
            throw new ValidationException(details);
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnprocessableEntityException("Invalid username or password.", "invalid_credentials");
        }

        var accessToken = _tokenService.GenerateJwtToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshToken = refreshTokenValue,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString()
            }
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            });
            throw new ValidationException(details);
        }

        var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiryTime <= DateTime.UtcNow)
        {
            throw new UnprocessableEntityException("Invalid or expired refresh token.", "invalid_refresh_token");
        }

        // Revoke current refresh token (rotation)
        existingToken.IsRevoked = true;
        await _userRepository.UpdateRefreshTokenAsync(existingToken, cancellationToken);

        var newAccessToken = _tokenService.GenerateJwtToken(existingToken.User);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = existingToken.UserId,
            Token = newRefreshTokenValue,
            ExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshToken = newRefreshTokenValue,
            User = new UserDto
            {
                Id = existingToken.User.Id,
                Username = existingToken.User.Username,
                FullName = existingToken.User.FullName,
                Role = existingToken.User.Role.ToString()
            }
        };
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            });
            throw new ValidationException(details);
        }

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new UnprocessableEntityException("Username is already taken.", "username_conflict");
        }

        var role = Enum.Parse<UserRole>(request.Role, true);
        var passwordHash = _passwordService.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = role
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return new UserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role.ToString(),
            user.CreatedAt
        );
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _logoutValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            });
            throw new ValidationException(details);
        }

        var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken != null && !existingToken.IsRevoked)
        {
            existingToken.IsRevoked = true;
            await _userRepository.UpdateRefreshTokenAsync(existingToken, cancellationToken);
        }
    }
}
