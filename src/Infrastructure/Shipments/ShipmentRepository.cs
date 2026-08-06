using Microsoft.EntityFrameworkCore;
using ShipSharp.Domain.Shipments;
using ShipSharp.Infrastructure.Data;

namespace ShipSharp.Infrastructure.Shipments;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.OriginPort)
            .Include(s => s.DestinationPort)
            .Include(s => s.Vessel)
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.OriginPort)
            .Include(s => s.DestinationPort)
            .Include(s => s.Vessel)
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.TrackingNumber.ToUpper() == trackingNumber.ToUpper(), cancellationToken);
    }

    public async Task<(IReadOnlyList<Shipment> Items, int TotalCount)> GetAllAsync(
        ShipmentFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.OriginPort)
            .Include(s => s.DestinationPort)
            .Include(s => s.Vessel)
            .Include(s => s.StatusHistories);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.Page - 1) * filter.PerPage)
            .Take(filter.PerPage)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .CountAsync(s => s.CreatedAt.Year == year, cancellationToken);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _context.Shipments.AddAsync(shipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(shipment).State == EntityState.Detached)
        {
            _context.Shipments.Update(shipment);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStatusHistoryAsync(ShipmentStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.ShipmentStatusHistories.AddAsync(history, cancellationToken);
    }
}
