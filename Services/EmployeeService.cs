using FirebaseAdmin.Auth;
using Liguria_Trasporti.Data;
using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Services;
[Authorize]
public class EmployeeService(AppDbContext dbContext, FirebaseAuth firebaseAuth) : IEmployeeService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly FirebaseAuth _firebaseAuth = firebaseAuth;

    public async Task<ServiceResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
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
        
        return ServiceResult<IEnumerable<EmployeeResponseDto>>.Ok(employees);
    }
    
    public async Task<ServiceResult<EmployeeResponseDto?>> GetEmployeeById(Guid id)
    {
        var resource = await _dbContext.Employees.FindAsync(id);
        if (resource is null)
        {
            return ServiceResult<EmployeeResponseDto?>.NotFound(null);
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
        return ServiceResult<EmployeeResponseDto?>.Ok(employee);
    }

    public async Task<ServiceResult<EmployeeResponseDto>> CreateEmployee(EmployeeRequestDto employeeRequest)
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
        
        var mailExists = await _dbContext.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id);
        if (mailExists)
        {
            return ServiceResult<EmployeeResponseDto>.Conflict(null!);
        }
        
        if (employee.Role is EmployeeRole.Driver)
        {
            employee.DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory;
            if (employee.DrivingLicenseCategory is null)
            {
                return ServiceResult<EmployeeResponseDto>.ValidationError();
            }
        }

        await _dbContext.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var userRecord = await _firebaseAuth.CreateUserAsync(new UserRecordArgs()
        {
            Email = employee.Email,
            Password = "temporary123."
        });
        
        var claims = new Dictionary<string, object> {{"role", employee.Role.ToString()}};
        await _firebaseAuth.SetCustomUserClaimsAsync(userRecord.Uid, claims);
            
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
        return ServiceResult<EmployeeResponseDto>.Ok(employeeResponse);
    }

    public async Task<ServiceResult<EmployeeResponseDto?>> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee is null)
        {
            return ServiceResult<EmployeeResponseDto?>.NotFound(null);
        }
        employee.Name = employeeRequest.Name.Trim();
        employee.Surname = employeeRequest.Surname.Trim();
        employee.Email = employeeRequest.Email.ToLowerInvariant().Trim();
        employee.Role = employeeRequest.Role;
        
        var mailExists = await _dbContext.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id);
        if (mailExists)
        {
            return ServiceResult<EmployeeResponseDto?>.Conflict(null);
        }
        
        if (employee.Role is EmployeeRole.Driver)
        {
            employee.DrivingLicenseCategory = employeeRequest.DrivingLicenseCategory;
            if (employee.DrivingLicenseCategory is null)
            {
                return ServiceResult<EmployeeResponseDto?>.ValidationError();
            }
        }
        await _dbContext.SaveChangesAsync();
        return ServiceResult<EmployeeResponseDto?>.Ok(new EmployeeResponseDto()
        {
            Id = employee.Id,
            Name = employee.Name,
            Surname = employee.Surname,
            Email = employee.Email,
            Role = employee.Role,
            OperationalStatus = employee.OperationalStatus,
            AccountStatus = employee.AccountStatus,
            DrivingLicenseCategory = employee.DrivingLicenseCategory
        });
    }

    public async Task<ServiceResult<EmptyResponse>> DeleteEmployee(Guid id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee is null)
        {
            return ServiceResult<EmptyResponse>.NotFound(null!);
        }
        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();
        return ServiceResult<EmptyResponse>.Ok(null!);
    }
}