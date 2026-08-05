using FluentValidation;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Vessels.DTOs;
using ShipSharp.Domain.Vessels;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Application.Vessels;

public class VesselService : IVesselService
{
    private readonly IVesselRepository _vesselRepository;
    private readonly IValidator<CreateVesselRequest> _createValidator;
    private readonly IValidator<UpdateVesselRequest> _updateValidator;

    public VesselService(
        IVesselRepository vesselRepository,
        IValidator<CreateVesselRequest> createValidator,
        IValidator<UpdateVesselRequest> updateValidator)
    {
        _vesselRepository = vesselRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<VesselResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vessel = await _vesselRepository.GetByIdAsync(id, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", id);
        }

        return MapToResponse(vessel);
    }

    public async Task<(IReadOnlyList<VesselResponse> Items, int TotalCount)> GetPagedAsync(
        bool? isActive, int page, int perPage, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _vesselRepository.GetPagedAsync(isActive, page, perPage, cancellationToken);
        var dtos = items.Select(MapToResponse).ToList();
        return (dtos, totalCount);
    }

    public async Task<VesselResponse> CreateAsync(CreateVesselRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            }));
        }

        var existing = await _vesselRepository.GetByImoNumberAsync(request.IMONumber, cancellationToken);
        if (existing != null)
        {
            throw new UnprocessableEntityException("A vessel with this IMO number already exists.", "duplicate_imo");
        }

        var vessel = new Vessel
        {
            Name = request.Name,
            IMONumber = request.IMONumber,
            Flag = request.Flag,
            Capacity = request.Capacity,
            IsActive = true
        };

        await _vesselRepository.AddAsync(vessel, cancellationToken);
        return MapToResponse(vessel);
    }

    public async Task<VesselResponse> UpdateAsync(Guid id, UpdateVesselRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            }));
        }

        var vessel = await _vesselRepository.GetByIdAsync(id, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", id);
        }

        vessel.Name = request.Name;
        vessel.Flag = request.Flag;
        vessel.Capacity = request.Capacity;
        vessel.UpdatedAt = DateTime.UtcNow;

        await _vesselRepository.UpdateAsync(vessel, cancellationToken);
        return MapToResponse(vessel);
    }

    public async Task<VesselResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vessel = await _vesselRepository.GetByIdAsync(id, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", id);
        }

        vessel.IsActive = true;
        vessel.UpdatedAt = DateTime.UtcNow;
        await _vesselRepository.UpdateAsync(vessel, cancellationToken);
        return MapToResponse(vessel);
    }

    public async Task<VesselResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vessel = await _vesselRepository.GetByIdAsync(id, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", id);
        }

        vessel.IsActive = false;
        vessel.UpdatedAt = DateTime.UtcNow;
        await _vesselRepository.UpdateAsync(vessel, cancellationToken);
        return MapToResponse(vessel);
    }

    private static VesselResponse MapToResponse(Vessel vessel) => new()
    {
        Id = vessel.Id,
        Name = vessel.Name,
        IMONumber = vessel.IMONumber,
        Flag = vessel.Flag,
        Capacity = vessel.Capacity,
        IsActive = vessel.IsActive,
        CreatedAt = vessel.CreatedAt,
        UpdatedAt = vessel.UpdatedAt
    };
}
