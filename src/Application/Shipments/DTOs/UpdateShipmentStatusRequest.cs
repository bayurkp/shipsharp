using System.Text.Json.Serialization;
using ShipSharp.Domain.Shipments;

namespace ShipSharp.Application.Shipments.DTOs;

public class UpdateShipmentStatusRequest
{
    [JsonPropertyName("status")]
    public ShipmentStatus Status { get; set; }
}
