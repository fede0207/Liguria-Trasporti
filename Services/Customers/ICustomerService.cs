using Liguria_Trasporti.DTOs.Customer;

namespace Liguria_Trasporti.Services.Customers;

public interface ICustomerService
{
    public Task<ServiceResult<IEnumerable<CustomerResponseDto>>> GetAllCustomers();
    public Task<ServiceResult<CustomerResponseDto?>> GetCustomerById(Guid id);
    public Task<ServiceResult<CustomerResponseDto>> CreateCustomer(CustomerRequestDto customerRequest);
}