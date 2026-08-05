namespace ShipSharp.Domain.Vessels;

public interface IVesselRepository
{
    Task<Vessel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vessel?> GetByImoNumberAsync(string imoNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vessel> Items, int TotalCount)> GetPagedAsync(
        bool? isActive, int page, int perPage, CancellationToken cancellationToken = default);
    Task AddAsync(Vessel vessel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vessel vessel, CancellationToken cancellationToken = default);
}
