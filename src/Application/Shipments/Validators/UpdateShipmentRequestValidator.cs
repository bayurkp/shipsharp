using FluentValidation;
using ShipSharp.Application.Shipments.DTOs;

namespace ShipSharp.Application.Shipments.Validators;

public class UpdateShipmentRequestValidator : AbstractValidator<UpdateShipmentRequest>
{
    public UpdateShipmentRequestValidator()
    {
        RuleFor(x => x.VesselId)
            .NotEmpty().WithMessage("Vessel is required.");

        RuleFor(x => x.EstimatedDeparture)
            .NotEmpty().WithMessage("Estimated departure date is required.");

        RuleFor(x => x.EstimatedArrival)
            .NotEmpty().WithMessage("Estimated arrival date is required.")
            .GreaterThan(x => x.EstimatedDeparture)
            .WithMessage("Estimated arrival date must be after estimated departure date.");
    }
}
