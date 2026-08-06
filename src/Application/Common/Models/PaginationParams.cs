namespace ShipSharp.Application.Common.Models;

public class PaginationParams
{
    private const int MaxPerPage = 100;
    private int _perPage = 10;

    public int Page { get; set; } = 1;

    public int PerPage
    {
        get => _perPage;
        set => _perPage = value > MaxPerPage ? MaxPerPage : value < 1 ? 10 : value;
    }
}
