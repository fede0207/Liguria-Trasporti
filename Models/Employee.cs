using System.ComponentModel.DataAnnotations;
using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.Models;

public class Employee
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public EmployeeOperationalStatus OperationalStatus { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DrivingLicenseCategory? DrivingLicenseCategory { get; set; }
}