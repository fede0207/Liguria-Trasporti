using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Microsoft.AspNetCore.Mvc;
using Liguria_Trasporti.Services.Shipments;
using Microsoft.AspNetCore.Authorization;

namespace Liguria_Trasporti.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "LogisticOperator, ShippingManager")]
public class ShipmentController(IShipmentService shipmentService) : ControllerBase
{
    private readonly IShipmentService _shipmentService = shipmentService;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _shipmentService.GetAllShipments();
        return Ok(result.Data);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _shipmentService.GetShipmentById(id);
        if (result.Status is ServiceResultStatus.NotFound)
        {
            return NotFound();
        }
        return Ok(result.Data);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateShipment(ShipmentRequestDto shipmentRequest)
    {
        var result = await _shipmentService.CreateShipment(shipmentRequest);
        if (result.Status is ServiceResultStatus.ValidationError)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [Authorize(Roles = "ShippingManager")]
    [HttpPatch("{shipmentId:Guid}/status")]
    public async Task<IActionResult> ValidateShipmentStatus(Guid shipmentId, ValidateShipmentRequestDto shipmentRequest)
    {
        var result = await _shipmentService.ValidateShipment(shipmentId, shipmentRequest);
        return result.Status switch
        {
            ServiceResultStatus.Success => Ok(result.Data),
            ServiceResultStatus.NotFound => NotFound(), 
            ServiceResultStatus.ValidationError => BadRequest(result.Data),
            _ => StatusCode(500, "An unexpected error occurred.")
        };
    }
}