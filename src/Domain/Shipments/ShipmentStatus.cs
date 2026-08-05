namespace ShipSharp.Domain.Shipments;

public enum ShipmentStatus
{
    Booked = 0,
    Loading = 1,
    Departed = 2,
    AtSea = 3,
    Arrived = 4,
    Delivered = 5
}
