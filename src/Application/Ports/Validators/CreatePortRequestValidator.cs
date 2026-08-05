using FluentValidation;
using ShipSharp.Application.Ports.DTOs;

namespace ShipSharp.Application.Ports.Validators;

public class CreatePortRequestValidator : AbstractValidator<CreatePortRequest>
{
    public CreatePortRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Port name is required.")
            .MaximumLength(100).WithMessage("Port name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Port code is required.")
            .Length(3, 10).WithMessage("Port code must be between 3 and 10 characters.")
            .Matches("^[A-Z]+$").WithMessage("Port code must contain uppercase letters only.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.");
    }
}
