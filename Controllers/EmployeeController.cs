using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Liguria_Trasporti.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
    {
        return Ok(await _employeeService.GetAllEmployees());
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDto?>> CreateEmployee(EmployeeRequestDto employeeRequest)
    {
        return Ok(await _employeeService.CreateEmployee(employeeRequest));
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<EmployeeResponseDto?>> GetEmployeeById(Guid id)
    {
        var employee = await _employeeService.GetEmployeeById(id);
        if (employee is null)
        {
            return NotFound();
        }
        return Created("{api/Employee/id}", employee);
    }
}