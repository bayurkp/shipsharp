using ShipSharp.Application.Shipments.DTOs;
using ShipSharp.Domain.Shipments;

namespace ShipSharp.Application.Shipments;

public interface IShipmentService
{
    Task<ShipmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShipmentTrackingResponse> TrackByNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ShipmentResponse> Items, int TotalCount)> GetPagedAsync(
        int page, int perPage, CancellationToken cancellationToken = default);
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, string createdByUsername, CancellationToken cancellationToken = default);
    Task<ShipmentResponse> UpdateAsync(Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken = default);
    Task<ShipmentResponse> UpdateStatusAsync(Guid id, ShipmentStatus newStatus, string updatedByUsername, CancellationToken cancellationToken = default);
}
