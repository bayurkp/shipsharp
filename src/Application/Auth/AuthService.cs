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
    private readonly IValidator<LoginRequest> _validator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IValidator<LoginRequest> validator)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _validator = validator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
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

        var token = _tokenService.GenerateJwtToken(user);

        return new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString()
            }
        };
    }
}
