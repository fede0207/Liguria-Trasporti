using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Models;

namespace Liguria_Trasporti.Services.Employers;

public interface IEmployeeService
{
    public Task<ServiceResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees();
    public Task<ServiceResult<EmployeeResponseDto?>> GetEmployeeById(Guid id);
    public Task<ServiceResult<EmployeeResponseDto>> CreateEmployee(EmployeeRequestDto employeeRequest, string nameIdentifier);
    public Task<ServiceResult<EmployeeResponseDto?>> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest);
    public Task<ServiceResult<EmptyResponse>> DeleteEmployee(Guid id);
}