using System.ComponentModel.DataAnnotations;

namespace Liguria_Trasporti.Models;

public class Customer
{
    public Guid Id { get; set; }
    [Required, MaxLength(524)]
    public string CompanyName { get; set; } = string.Empty;
    [Required, Phone, MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? Notes { get; set; }
}