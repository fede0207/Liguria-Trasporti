using Liguria_Trasporti.Data;
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
}