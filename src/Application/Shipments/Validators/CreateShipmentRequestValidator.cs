using FluentValidation;
using ShipSharp.Application.Shipments.DTOs;

namespace ShipSharp.Application.Shipments.Validators;

public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(x => x.OriginPortId)
            .NotEmpty().WithMessage("Origin port is required.");

        RuleFor(x => x.DestinationPortId)
            .NotEmpty().WithMessage("Destination port is required.")
            .Must((request, destPortId) => destPortId != request.OriginPortId)
            .WithMessage("Origin and destination ports must be different.");

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
