using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Models;

namespace Liguria_Trasporti.Services;

public interface IEmployeeService
{
    public Task<IEnumerable<EmployeeResponseDto>> GetAllEmployees();
    public Task<EmployeeResponseDto?> GetEmployeeById(Guid id);
    public Task<EmployeeResponseDto?> CreateEmployee(EmployeeRequestDto employeeRequest);
}