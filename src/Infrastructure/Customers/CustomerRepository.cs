using Microsoft.EntityFrameworkCore;
using ShipSharp.Domain.Customers;
using ShipSharp.Infrastructure.Data;

namespace ShipSharp.Infrastructure.Customers;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetAllAsync(
        CustomerFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) || c.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PerPage)
            .Take(filter.PerPage)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
