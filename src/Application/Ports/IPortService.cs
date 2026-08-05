using ShipSharp.Application.Ports.DTOs;

namespace ShipSharp.Application.Ports;

public interface IPortService
{
    Task<PortResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PortResponse> CreateAsync(CreatePortRequest request, CancellationToken cancellationToken = default);
}
