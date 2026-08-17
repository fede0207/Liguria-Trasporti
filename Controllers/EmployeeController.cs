using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liguria_Trasporti.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EmployeeManager")]
public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
    {
        var result = await _employeeService.GetAllEmployees();
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(EmployeeRequestDto employeeRequest)
    {
        var result = await _employeeService.CreateEmployee(employeeRequest);
        if (result.Status is ServiceResultStatus.Conflict)
        {
            return Conflict();
        }
        if (result.Status is ServiceResultStatus.ValidationError)
        {
            return BadRequest();
        }
        return CreatedAtRoute("GetEmployeeById", new {id = result.Data!.Id}, result.Data );
    }

    [HttpGet("{id:Guid}", Name = "GetEmployeeById")]
    public async Task<ActionResult<EmployeeResponseDto?>> GetEmployeeById(Guid id)
    {
        var result = await _employeeService.GetEmployeeById(id);
        if (result.Status is ServiceResultStatus.NotFound)
        {
            return NotFound();
        }
        return Ok(result.Data);
    }

    [HttpPut("{id:Guid}")]
    public async Task<ActionResult> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest)
    {
        var result = await _employeeService.UpdateEmployee(id, employeeRequest);
        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(),
            ServiceResultStatus.Conflict => Conflict(),
            ServiceResultStatus.ValidationError => BadRequest(),
            _ => NoContent()
        };
    }
    
    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult> RemoveEmployee(Guid id)
    {
        var result = await _employeeService.DeleteEmployee(id);
        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(),
            _ => NoContent()
        };
    }
}