using ShipSharp.Application.Common.Models;

namespace ShipSharp.Application.Customers.DTOs;

public class GetCustomersRequest : PaginationParams
{
    public string? Search { get; set; }
}
