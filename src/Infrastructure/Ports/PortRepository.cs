using Microsoft.EntityFrameworkCore;
using ShipSharp.Domain.Ports;
using ShipSharp.Infrastructure.Data;

namespace ShipSharp.Infrastructure.Ports;

public class PortRepository : IPortRepository
{
    private readonly AppDbContext _context;

    public PortRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Port?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ports.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Port?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Ports.FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper(), cancellationToken);
    }

    public async Task<IReadOnlyList<Port>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Ports.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Port port, CancellationToken cancellationToken = default)
    {
        await _context.Ports.AddAsync(port, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
