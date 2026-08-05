using Microsoft.EntityFrameworkCore;
using ShipSharp.Domain.Vessels;
using ShipSharp.Infrastructure.Data;

namespace ShipSharp.Infrastructure.Vessels;

public class VesselRepository : IVesselRepository
{
    private readonly AppDbContext _context;

    public VesselRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vessel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vessels.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vessel?> GetByImoNumberAsync(string imoNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Vessels.FirstOrDefaultAsync(v => v.IMONumber.ToUpper() == imoNumber.ToUpper(), cancellationToken);
    }

    public async Task<(IReadOnlyList<Vessel> Items, int TotalCount)> GetPagedAsync(
        bool? isActive, int page, int perPage, CancellationToken cancellationToken = default)
    {
        var query = _context.Vessels.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(v => v.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Vessel vessel, CancellationToken cancellationToken = default)
    {
        await _context.Vessels.AddAsync(vessel, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Vessel vessel, CancellationToken cancellationToken = default)
    {
        _context.Vessels.Update(vessel);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
