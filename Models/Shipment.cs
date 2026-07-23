using System.ComponentModel.DataAnnotations;

namespace Liguria_Trasporti.Models;

public class Shipment
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
    [MaxLength(255)]
    public string? Note { get; set; } 
    // enum priorita
    // enum stato
    // enum stato giro
    
    
}