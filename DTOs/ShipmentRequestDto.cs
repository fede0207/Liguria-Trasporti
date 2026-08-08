using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class ShipmentRequestDto
{
    public Guid CustomerId { get; set; }
    public Guid OriginAddressId { get; set; }
    public Guid DestinationAddressId { get; set; }
    public DateTime PlannedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public Guid? ProposedDriverId { get; set; }
    public Guid? ProposedVehicleId { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedVehicleId { get; set; }
    public Guid? DriverRouteId { get; set; }
    public string? Note { get; set; }
    public ShipmentPriority Priority { get; set; }
    public ShipmentStatus Status { get; set; } 
}