using FluentValidation;
using ShipSharp.Application.Auth.DTOs;

namespace ShipSharp.Application.Auth.Validators;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
