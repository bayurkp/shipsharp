namespace ShipSharp.Domain.Shipments;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Shipment> Items, int TotalCount)> GetAllAsync(
        ShipmentFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(ShipmentStatusHistory history, CancellationToken cancellationToken = default);
}
