using Liguria_Trasporti.Data;
using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Models;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Services;

public class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Employee>> GetAllEmployees()
    {
        return await _dbContext.Employees.ToListAsync();
    }
    
    public async Task<Employee?> GetEmployeeById(Guid id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee is null)
        {
            return null;
        }
        
        return employee;
    }

    public async Task<EmployeeResponseDto?> CreateEmployee(EmployeeRequestDto employeeRequest)
    {
        var employee = new Employee()
        {
            Id = Guid.NewGuid(),
            Name = employeeRequest.Name,
            Surname = employeeRequest.Surname,
            Email = employeeRequest.Email,
            Role = employeeRequest.Role,
            DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory,
            AccountStatus = AccountStatus.Active,
            OperationalStatus = EmployeeOperationalStatus.Active
        };
        
        if (string.IsNullOrWhiteSpace(employeeRequest.Name))
        {}

        if (employee.Role is EmployeeRole.Driver)
        {
            employee.DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory;
        }

        await _dbContext.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var employeeResponse = new EmployeeResponseDto()
        {
            Name = employee.Name,
            Surname = employee.Surname,
            Email = employee.Email,
            Role = employee.Role,
            OperationalStatus = employee.OperationalStatus,
            AccountStatus = employee.AccountStatus,
            DrivingLicenseCategory = employee.DrivingLicenseCategory
        };

        return employeeResponse;
    }
}