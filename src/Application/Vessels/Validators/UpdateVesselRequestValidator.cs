using FluentValidation;
using ShipSharp.Application.Vessels.DTOs;

namespace ShipSharp.Application.Vessels.Validators;

public class UpdateVesselRequestValidator : AbstractValidator<UpdateVesselRequest>
{
    public UpdateVesselRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vessel name is required.");

        RuleFor(x => x.Flag)
            .NotEmpty().WithMessage("Flag country is required.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");
    }
}
