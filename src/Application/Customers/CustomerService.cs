using FluentValidation;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Customers.DTOs;
using ShipSharp.Domain.Customers;
using ValidationException = ShipSharp.Application.Common.Exceptions.ValidationException;

namespace ShipSharp.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator)
    {
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new NotFoundException("Customer", id);
        }

        return MapToResponse(customer);
    }

    public async Task<(IReadOnlyList<CustomerResponse> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm, int page, int perPage, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _customerRepository.GetPagedAsync(searchTerm, page, perPage, cancellationToken);
        var dtos = items.Select(MapToResponse).ToList();
        return (dtos, totalCount);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            }));
        }

        var existing = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            throw new UnprocessableEntityException("A customer with this email address already exists.", "duplicate_email");
        }

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.Select(e => new ApiErrorDetail
            {
                Field = e.PropertyName,
                Code = e.ErrorCode,
                Message = e.ErrorMessage
            }));
        }

        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new NotFoundException("Customer", id);
        }

        var existingWithEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingWithEmail != null && existingWithEmail.Id != id)
        {
            throw new UnprocessableEntityException("A customer with this email address already exists.", "duplicate_email");
        }

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Address = request.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        return MapToResponse(customer);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new NotFoundException("Customer", id);
        }

        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer, cancellationToken);
    }

    private static CustomerResponse MapToResponse(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        Address = customer.Address,
        CreatedAt = customer.CreatedAt,
        UpdatedAt = customer.UpdatedAt
    };
}
