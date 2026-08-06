using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipSharp.API.Common.Extensions;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Customers;
using ShipSharp.Application.Customers.DTOs;

namespace ShipSharp.API.Customers;

[ApiController]
[Route("customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetAll([FromQuery] GetCustomersRequest query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _customerService.GetAllAsync(query, cancellationToken);
        return this.OkPaged(items, query.Page, query.PerPage, totalCount);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Success(customer));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ApiResponse<CustomerResponse>.Success(customer));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Success(customer));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customerService.SoftDeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object?>.Success(null));
    }
}
