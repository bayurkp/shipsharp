using ShipSharp.Application.Vessels.DTOs;

namespace ShipSharp.Application.Vessels;

public interface IVesselService
{
    Task<VesselResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<VesselResponse> Items, int TotalCount)> GetPagedAsync(
        bool? isActive, int page, int perPage, CancellationToken cancellationToken = default);
    Task<VesselResponse> CreateAsync(CreateVesselRequest request, CancellationToken cancellationToken = default);
    Task<VesselResponse> UpdateAsync(Guid id, UpdateVesselRequest request, CancellationToken cancellationToken = default);
    Task<VesselResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VesselResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
