namespace ShipSharp.Domain.Vessels;

public interface IVesselRepository
{
    Task<Vessel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vessel?> GetByImoNumberAsync(string imoNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vessel> Items, int TotalCount)> GetAllAsync(
        VesselFilter filter, CancellationToken cancellationToken = default);
    Task AddAsync(Vessel vessel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vessel vessel, CancellationToken cancellationToken = default);
}
