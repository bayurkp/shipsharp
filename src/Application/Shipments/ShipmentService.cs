using FluentValidation;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Customers.DTOs;
using ShipSharp.Application.Ports.DTOs;
using ShipSharp.Application.Shipments.DTOs;
using ShipSharp.Application.Vessels.DTOs;
using ShipSharp.Domain.Customers;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Shipments;
using ShipSharp.Domain.Vessels;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Application.Shipments;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPortRepository _portRepository;
    private readonly IVesselRepository _vesselRepository;
    private readonly IValidator<CreateShipmentRequest> _createValidator;
    private readonly IValidator<UpdateShipmentRequest> _updateValidator;

    public ShipmentService(
        IShipmentRepository shipmentRepository,
        ICustomerRepository customerRepository,
        IPortRepository portRepository,
        IVesselRepository vesselRepository,
        IValidator<CreateShipmentRequest> createValidator,
        IValidator<UpdateShipmentRequest> updateValidator)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
        _portRepository = portRepository;
        _vesselRepository = vesselRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ShipmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id, cancellationToken);
        if (shipment == null)
        {
            throw new NotFoundException("Shipment", id);
        }

        return MapToResponse(shipment);
    }

    public async Task<ShipmentTrackingResponse> TrackByNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByTrackingNumberAsync(trackingNumber, cancellationToken);
        if (shipment == null)
        {
            throw new NotFoundException($"Shipment with tracking number '{trackingNumber}' was not found.");
        }

        return new ShipmentTrackingResponse
        {
            TrackingNumber = shipment.TrackingNumber,
            CurrentStatus = shipment.Status.ToString(),
            EstimatedArrival = shipment.EstimatedArrival,
            OriginPort = $"{shipment.OriginPort.Name} ({shipment.OriginPort.Code})",
            DestinationPort = $"{shipment.DestinationPort.Name} ({shipment.DestinationPort.Code})",
            VesselName = shipment.Vessel.Name,
            History = shipment.StatusHistories
                .OrderBy(h => h.Timestamp)
                .Select(h => new ShipmentStatusHistoryResponse
                {
                    Id = h.Id,
                    PreviousStatus = h.PreviousStatus?.ToString(),
                    CurrentStatus = h.CurrentStatus.ToString(),
                    UpdatedBy = h.UpdatedBy,
                    Timestamp = h.Timestamp
                }).ToList()
        };
    }

    public async Task<(IReadOnlyList<ShipmentResponse> Items, int TotalCount)> GetPagedAsync(
        int page, int perPage, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _shipmentRepository.GetPagedAsync(page, perPage, cancellationToken);
        var dtos = items.Select(MapToResponse).ToList();
        return (dtos, totalCount);
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, string createdByUsername, CancellationToken cancellationToken = default)
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

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new NotFoundException("Customer", request.CustomerId);
        }

        var originPort = await _portRepository.GetByIdAsync(request.OriginPortId, cancellationToken);
        if (originPort == null)
        {
            throw new NotFoundException("Origin Port", request.OriginPortId);
        }

        var destinationPort = await _portRepository.GetByIdAsync(request.DestinationPortId, cancellationToken);
        if (destinationPort == null)
        {
            throw new NotFoundException("Destination Port", request.DestinationPortId);
        }

        var vessel = await _vesselRepository.GetByIdAsync(request.VesselId, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", request.VesselId);
        }

        if (!vessel.IsActive)
        {
            throw new UnprocessableEntityException("Cannot assign an inactive vessel to a shipment.", "inactive_vessel");
        }

        var currentYear = DateTime.UtcNow.Year;
        var yearlyCount = await _shipmentRepository.GetCountForYearAsync(currentYear, cancellationToken);
        var trackingNumber = $"SHP-{currentYear}{(yearlyCount + 1):D4}";

        var shipment = new Shipment
        {
            TrackingNumber = trackingNumber,
            CustomerId = customer.Id,
            Customer = customer,
            OriginPortId = originPort.Id,
            OriginPort = originPort,
            DestinationPortId = destinationPort.Id,
            DestinationPort = destinationPort,
            VesselId = vessel.Id,
            Vessel = vessel,
            Status = ShipmentStatus.Booked,
            EstimatedDeparture = request.EstimatedDeparture,
            EstimatedArrival = request.EstimatedArrival,
            Notes = request.Notes
        };

        var initialHistory = new ShipmentStatusHistory
        {
            ShipmentId = shipment.Id,
            PreviousStatus = null,
            CurrentStatus = ShipmentStatus.Booked,
            UpdatedBy = createdByUsername,
            Timestamp = DateTime.UtcNow
        };

        shipment.StatusHistories.Add(initialHistory);

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse> UpdateAsync(Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken = default)
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

        var shipment = await _shipmentRepository.GetByIdAsync(id, cancellationToken);
        if (shipment == null)
        {
            throw new NotFoundException("Shipment", id);
        }

        if (shipment.Status == ShipmentStatus.Delivered)
        {
            throw new UnprocessableEntityException("Delivered shipments are immutable and cannot be updated.", "shipment_immutable");
        }

        var vessel = await _vesselRepository.GetByIdAsync(request.VesselId, cancellationToken);
        if (vessel == null)
        {
            throw new NotFoundException("Vessel", request.VesselId);
        }

        if (!vessel.IsActive)
        {
            throw new UnprocessableEntityException("Cannot assign an inactive vessel to a shipment.", "inactive_vessel");
        }

        shipment.VesselId = vessel.Id;
        shipment.Vessel = vessel;
        shipment.EstimatedDeparture = request.EstimatedDeparture;
        shipment.EstimatedArrival = request.EstimatedArrival;
        shipment.Notes = request.Notes;
        shipment.UpdatedAt = DateTime.UtcNow;

        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse> UpdateStatusAsync(Guid id, ShipmentStatus newStatus, string updatedByUsername, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id, cancellationToken);
        if (shipment == null)
        {
            throw new NotFoundException("Shipment", id);
        }

        if (shipment.Status == ShipmentStatus.Delivered)
        {
            throw new UnprocessableEntityException("Delivered shipments are immutable and cannot change status further.", "shipment_immutable");
        }

        if ((int)newStatus != (int)shipment.Status + 1)
        {
            throw new UnprocessableEntityException(
                $"Invalid status transition from {shipment.Status} to {newStatus}. Status transitions must be strictly sequential.",
                "invalid_status_transition");
        }

        var previousStatus = shipment.Status;
        shipment.Status = newStatus;
        shipment.UpdatedAt = DateTime.UtcNow;

        var historyEntry = new ShipmentStatusHistory
        {
            ShipmentId = shipment.Id,
            PreviousStatus = previousStatus,
            CurrentStatus = newStatus,
            UpdatedBy = updatedByUsername,
            Timestamp = DateTime.UtcNow
        };

        await _shipmentRepository.AddStatusHistoryAsync(historyEntry, cancellationToken);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

        return MapToResponse(shipment);
    }

    private static ShipmentResponse MapToResponse(Shipment s) => new()
    {
        Id = s.Id,
        TrackingNumber = s.TrackingNumber,
        Status = s.Status.ToString(),
        Customer = s.Customer != null ? new CustomerResponse
        {
            Id = s.Customer.Id,
            Name = s.Customer.Name,
            Email = s.Customer.Email,
            Phone = s.Customer.Phone,
            Address = s.Customer.Address,
            CreatedAt = s.Customer.CreatedAt,
            UpdatedAt = s.Customer.UpdatedAt
        } : null!,
        OriginPort = s.OriginPort != null ? new PortResponse
        {
            Id = s.OriginPort.Id,
            Name = s.OriginPort.Name,
            Code = s.OriginPort.Code,
            Country = s.OriginPort.Country,
            CreatedAt = s.OriginPort.CreatedAt
        } : null!,
        DestinationPort = s.DestinationPort != null ? new PortResponse
        {
            Id = s.DestinationPort.Id,
            Name = s.DestinationPort.Name,
            Code = s.DestinationPort.Code,
            Country = s.DestinationPort.Country,
            CreatedAt = s.DestinationPort.CreatedAt
        } : null!,
        Vessel = s.Vessel != null ? new VesselResponse
        {
            Id = s.Vessel.Id,
            Name = s.Vessel.Name,
            IMONumber = s.Vessel.IMONumber,
            Flag = s.Vessel.Flag,
            Capacity = s.Vessel.Capacity,
            IsActive = s.Vessel.IsActive,
            CreatedAt = s.Vessel.CreatedAt,
            UpdatedAt = s.Vessel.UpdatedAt
        } : null!,
        EstimatedDeparture = s.EstimatedDeparture,
        EstimatedArrival = s.EstimatedArrival,
        Notes = s.Notes,
        History = s.StatusHistories != null
            ? s.StatusHistories.OrderBy(h => h.Timestamp).Select(h => new ShipmentStatusHistoryResponse
            {
                Id = h.Id,
                PreviousStatus = h.PreviousStatus?.ToString(),
                CurrentStatus = h.CurrentStatus.ToString(),
                UpdatedBy = h.UpdatedBy,
                Timestamp = h.Timestamp
            }).ToList()
            : new List<ShipmentStatusHistoryResponse>(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
