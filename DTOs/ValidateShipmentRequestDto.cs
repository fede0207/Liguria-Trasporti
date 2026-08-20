using System.ComponentModel.DataAnnotations;

namespace Liguria_Trasporti.DTOs;

public class ValidateShipmentRequestDto
{
    [Required]
    public Guid? AssignedDriverId { get; set; }
    [Required]
    public Guid? AssignedVehicleId { get; set; }
}