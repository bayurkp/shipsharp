using FluentValidation;
using ShipSharp.Application.Auth.DTOs;

namespace ShipSharp.Application.Auth.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .WithErrorCode("required_field");
    }
}
