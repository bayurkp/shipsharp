using ShipSharp.Application.Common.Models;

namespace ShipSharp.Application.Vessels.DTOs;

public class GetVesselsRequest : PaginationParams
{
    public bool? IsActive { get; set; }
}
