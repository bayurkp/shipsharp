using ShipSharp.Domain.Common;
using ShipSharp.Domain.Customers;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Vessels;

namespace ShipSharp.Domain.Shipments;

public class Shipment : BaseAuditableEntity
{
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid OriginPortId { get; set; }
    public Guid DestinationPortId { get; set; }
    public Guid VesselId { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Booked;
    public DateTime EstimatedDeparture { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public string? Notes { get; set; }

    public Customer Customer { get; set; } = null!;
    public Port OriginPort { get; set; } = null!;
    public Port DestinationPort { get; set; } = null!;
    public Vessel Vessel { get; set; } = null!;
    public ICollection<ShipmentStatusHistory> StatusHistories { get; set; } = new List<ShipmentStatusHistory>();
}
