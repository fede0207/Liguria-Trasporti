using Liguria_Trasporti.Models;
using Liguria_Trasporti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Liguria_Trasporti.Controllers;

public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
    {
        return Ok(await _employeeService.GetAllEmployees());
    }
}