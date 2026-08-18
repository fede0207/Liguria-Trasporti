using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public string? FirebaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public EmployeeOperationalStatus OperationalStatus { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DrivingLicenseCategory? DrivingLicenseCategory { get; set; }
}