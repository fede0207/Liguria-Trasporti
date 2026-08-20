using System.ComponentModel.DataAnnotations;
using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class ShipmentResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; } 
    public Guid OriginAddressId { get; set; }
    public Guid DestinationAddressId { get; set; }
    public DateTime? PlannedDeliveryDate { get; set; } 
    // nullable perché la data di consegna potrá
    // essere decisa in un secondo momento
    public DateTime? DeliveredAt { get; set; }
    public Guid? ProposedDriverId { get; set; }
    public Guid? ProposedVehicleId { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedVehicleId { get; set; }
    public Guid? DriverRouteId { get; set; }
    public string? Note { get; set; }

    public ShipmentPriority Priority { get; set; } = ShipmentPriority.Medium;
    public ShipmentStatus Status { get; set; }
    
}