namespace ShipSharp.Domain.Ports;

public interface IPortRepository
{
    Task<Port?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Port?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Port>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Port port, CancellationToken cancellationToken = default);
}
