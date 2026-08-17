using Liguria_Trasporti.Data;
using Liguria_Trasporti.DTOs.Customer;
using Microsoft.EntityFrameworkCore;
using Liguria_Trasporti.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Liguria_Trasporti.Services.Customers;

public class CustomerService(AppDbContext dbContext) : ICustomerService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<ServiceResult<IEnumerable<CustomerResponseDto>>> GetAllCustomers()
    {
        var customers = await _dbContext.Customers.ToListAsync();
        var response = customers.Select(c => new CustomerResponseDto()
        {
            Id = c.Id,
            CompanyName = c.CompanyName,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Notes = c.Notes
        });
        return ServiceResult<IEnumerable<CustomerResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<CustomerResponseDto?>> GetCustomerById(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer is null)
        {
            return ServiceResult<CustomerResponseDto?>.NotFound(null!);
        }

        var response = new CustomerResponseDto()
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Notes = customer.Notes
        };
        return ServiceResult<CustomerResponseDto?>.Ok(response);
    }
    
    public async Task<ServiceResult<CustomerResponseDto>> CreateCustomer(CustomerRequestDto customerRequest)
    {
        var customer = new Customer()
        {
            Id = Guid.NewGuid(),
            CompanyName = customerRequest.CompanyName,
            Notes = customerRequest.Notes
        };
        
        var emailExists = await _dbContext.Customers.AnyAsync(c => c.Email == customerRequest.Email);
        if (emailExists)
        {
            return ServiceResult<CustomerResponseDto>.Conflict(null!);
        }
        
        var phoneNumberExists = await _dbContext.Customers.AnyAsync(c => c.PhoneNumber == customerRequest.PhoneNumber);
        if (phoneNumberExists)
        {
            return ServiceResult<CustomerResponseDto>.Conflict(null!);
        }
        
        customer.PhoneNumber = customerRequest.PhoneNumber;
        customer.Email = customerRequest.Email;

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var response = new CustomerResponseDto()
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Notes = customer.Notes
        };
        return ServiceResult<CustomerResponseDto>.Ok(response);   
    }
}