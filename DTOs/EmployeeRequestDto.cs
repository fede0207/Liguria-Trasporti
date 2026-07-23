using System.ComponentModel.DataAnnotations;
using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class EmployeeRequestDto
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public DrivingLicenseCategory? DrivingLicenseCategory { get; set; }
}