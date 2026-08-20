using System.ComponentModel.DataAnnotations;
using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class ShipmentRequestDto
{
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public Guid OriginAddressId { get; set; }
    [Required]
    public Guid DestinationAddressId { get; set; }
    [Required]
    public DateTime PlannedDeliveryDate { get; set; }
    [Required]
    public DateTime? DeliveredAt { get; set; }
    [Required]
    public Guid? ProposedDriverId { get; set; }
    [Required]
    public Guid? ProposedVehicleId { get; set; }
    [Required]
    public Guid? AssignedDriverId { get; set; }
    [Required]
    public Guid? AssignedVehicleId { get; set; }
    [Required]
    public Guid? DriverRouteId { get; set; }
    [MaxLength(255)]
    public string? Note { get; set; }
    [Required]
    public ShipmentPriority Priority { get; set; }
    [Required]
    public ShipmentStatus Status { get; set; } 
}