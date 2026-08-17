using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid? DriverId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public double MaxCapacity { get; set; }
    public double AvailableCapacity { get; set; }
    public DrivingLicenseCategory RequiredLicenseCategory { get; set; }
    public VehicleStatus Status { get; set; }
}