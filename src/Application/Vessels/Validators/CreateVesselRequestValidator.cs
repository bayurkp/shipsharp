using FluentValidation;
using ShipSharp.Application.Vessels.DTOs;

namespace ShipSharp.Application.Vessels.Validators;

public class CreateVesselRequestValidator : AbstractValidator<CreateVesselRequest>
{
    public CreateVesselRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vessel name is required.");

        RuleFor(x => x.IMONumber)
            .NotEmpty().WithMessage("IMO number is required.")
            .Matches("^IMO[0-9]{7}$").WithMessage("IMO number must follow format IMO followed by 7 digits (e.g. IMO9123456).");

        RuleFor(x => x.Flag)
            .NotEmpty().WithMessage("Flag country is required.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");
    }
}
