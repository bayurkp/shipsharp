using ShipSharp.Domain.Common;

namespace ShipSharp.Domain.Shipments;

public class ShipmentStatusHistory : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public ShipmentStatus? PreviousStatus { get; set; }
    public ShipmentStatus CurrentStatus { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Shipment Shipment { get; set; } = null!;
}
