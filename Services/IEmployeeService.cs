using Liguria_Trasporti.Models;

namespace Liguria_Trasporti.Services;

public interface IEmployeeService
{
    public Task<IEnumerable<Employee>> GetAllEmployees();
}