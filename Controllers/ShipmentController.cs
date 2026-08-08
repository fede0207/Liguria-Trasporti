using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Microsoft.AspNetCore.Mvc;
using Liguria_Trasporti.Services;
using Microsoft.AspNetCore.Authorization;

namespace Liguria_Trasporti.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentController(IShipmentService shipmentService) : ControllerBase
{
    private readonly IShipmentService _shipmentService = shipmentService;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _shipmentService.GetAllShipments();
        return Ok(result);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _shipmentService.GetShipmentById(id);
        if (result.Status is ServiceResultStatus.NotFound)
        {
            return NotFound();
        }
        return Ok(result);
    }
    
    public async Task<IActionResult> CreateShipment(ShipmentRequestDto shipmentRequest)
    {
        var userRole = User.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
        var result = await _shipmentService.CreateShipment(shipmentRequest, userRole!);
        if (result.Status is ServiceResultStatus.Unauthorized)
        {
            return Unauthorized();
        }
        if (result.Status is ServiceResultStatus.ValidationError)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }
}