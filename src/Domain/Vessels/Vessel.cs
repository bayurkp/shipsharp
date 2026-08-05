using ShipSharp.Domain.Common;

namespace ShipSharp.Domain.Vessels;

public class Vessel : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string IMONumber { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}
