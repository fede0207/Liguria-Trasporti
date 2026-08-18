using System.ComponentModel.DataAnnotations;
using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.DTOs;

public class EmployeeRequestDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public DrivingLicenseCategory? DrivingLicenseCategory { get; set; }
    public AccountStatus AccountStatus { get; set; }
}