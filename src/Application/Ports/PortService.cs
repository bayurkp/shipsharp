using FluentValidation;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Ports.DTOs;
using ShipSharp.Domain.Ports;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Application.Ports;

public class PortService : IPortService
{
    private readonly IPortRepository _portRepository;
    private readonly IValidator<CreatePortRequest> _validator;

    public PortService(
        IPortRepository portRepository,
        IValidator<CreatePortRequest> validator)
    {
        _portRepository = portRepository;
        _validator = validator;
    }

    public async Task<PortResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var port = await _portRepository.GetByIdAsync(id, cancellationToken);
        if (port == null)
        {
            throw new NotFoundException("Port", id);
        }

        return MapToResponse(port);
    }

    public async Task<IReadOnlyList<PortResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ports = await _portRepository.GetAllAsync(cancellationToken);
        return ports.Select(MapToResponse).ToList();
    }

    public async Task<PortResponse> CreateAsync(CreatePortRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            }));
        }

        var existing = await _portRepository.GetByCodeAsync(request.Code.ToUpperInvariant(), cancellationToken);
        if (existing != null)
        {
            throw new UnprocessableEntityException("A port with this code already exists.", "duplicate_port_code");
        }

        var port = new Port
        {
            Name = request.Name,
            Code = request.Code.ToUpperInvariant(),
            Country = request.Country
        };

        await _portRepository.AddAsync(port, cancellationToken);
        return MapToResponse(port);
    }

    private static PortResponse MapToResponse(Port port) => new()
    {
        Id = port.Id,
        Name = port.Name,
        Code = port.Code,
        Country = port.Country,
        CreatedAt = port.CreatedAt
    };
}
