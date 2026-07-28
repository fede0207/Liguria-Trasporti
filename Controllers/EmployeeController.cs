using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Services;
using Microsoft.AspNetCore.Authorization;
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
        var employee = await _employeeService.CreateEmployee(employeeRequest);
        if (employee is null)
        {
            return BadRequest();
        }
        return CreatedAtRoute("GetEmployeeById", new {id = employee.Id}, employee );
    }
    
    [Authorize]
    [HttpGet("{id:Guid}", Name = "GetEmployeeById")]
    public async Task<ActionResult<EmployeeResponseDto?>> GetEmployeeById(Guid id)
    {
        var employee = await _employeeService.GetEmployeeById(id);
        if (employee is null)
        {
            return NotFound();
        }
        return Ok(employee);
    }

    [Authorize]
    [HttpPut("{id:Guid}")]
    public async Task<ActionResult> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest)
    {
        var result = await _employeeService.UpdateEmployee(id, employeeRequest);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult> RemoveEmployee(Guid id)
    {
        var result = await _employeeService.DeleteEmployee(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}