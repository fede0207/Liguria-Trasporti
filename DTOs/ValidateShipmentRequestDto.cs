namespace Liguria_Trasporti.DTOs;

public class ValidateShipmentRequestDto
{
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedVehicleId { get; set; }
}