namespace Liguria_Trasporti.DTOs.Customer;

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
}