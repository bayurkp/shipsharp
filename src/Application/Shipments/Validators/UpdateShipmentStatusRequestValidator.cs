using FluentValidation;
using ShipSharp.Application.Shipments.DTOs;

namespace ShipSharp.Application.Shipments.Validators;

public class UpdateShipmentStatusRequestValidator : AbstractValidator<UpdateShipmentStatusRequest>
{
    public UpdateShipmentStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid shipment status.");
    }
}
