using ShipSharp.Application.Customers.DTOs;

namespace ShipSharp.Application.Customers;

public interface ICustomerService
{
    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CustomerResponse> Items, int TotalCount)> GetAllAsync(
        GetCustomersRequest query, CancellationToken cancellationToken = default);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
