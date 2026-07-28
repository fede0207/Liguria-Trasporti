using Liguria_Trasporti.Data;
using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Models;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Services;

public class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployees()
    {
        var list =  await _dbContext.Employees.ToListAsync();
        var employees = list.Select(e => new EmployeeResponseDto()
        {
            Id = e.Id,
            Name = e.Name,
            Surname = e.Surname,
            Email = e.Email,
            Role = e.Role,
            OperationalStatus = e.OperationalStatus,
            AccountStatus = e.AccountStatus,
            DrivingLicenseCategory = e.DrivingLicenseCategory
        });
        return employees;
    }
    
    public async Task<EmployeeResponseDto?> GetEmployeeById(Guid id)
    {
        var resource = await _dbContext.Employees.FindAsync(id);
        if (resource is null)
        {
            return null;
        }

        var employee = new EmployeeResponseDto()
        {
            Id = resource.Id,
            Name = resource.Name,
            Surname = resource.Surname,
            Email = resource.Email,
            Role = resource.Role,
            OperationalStatus = resource.OperationalStatus,
            AccountStatus = resource.AccountStatus,
            DrivingLicenseCategory = resource.DrivingLicenseCategory
        };
        return employee;
    }

    public async Task<EmployeeResponseDto?> CreateEmployee(EmployeeRequestDto employeeRequest)
    {
        var employee = new Employee()
        {
            Id = Guid.NewGuid(),
            Name = employeeRequest.Name.Trim(),
            Surname = employeeRequest.Surname.Trim(),
            Email = employeeRequest.Email.ToLowerInvariant().Trim(),
            Role = employeeRequest.Role,
            DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory,
            AccountStatus = AccountStatus.Active,
            OperationalStatus = EmployeeOperationalStatus.Active
        };

        if (employee.Role is EmployeeRole.Driver)
        {
            employee.DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory;
            if (employee.DrivingLicenseCategory is null)
            {
                return null;
            }
        }

        await _dbContext.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var employeeResponse = new EmployeeResponseDto()
        {
            Id = employee.Id,
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

    public async Task<bool> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee is null)
        {
            return false;
        }
        employee.Name = employeeRequest.Name.Trim();
        employee.Surname = employeeRequest.Surname.Trim();
        employee.Email = employeeRequest.Email.ToLowerInvariant().Trim();
        employee.Role = employeeRequest.Role;
        employee.DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEmployee(Guid id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee is null)
        {
            return false;
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}