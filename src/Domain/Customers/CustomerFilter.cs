namespace ShipSharp.Domain.Customers;

public record CustomerFilter(string? Search, int Page, int PerPage);
