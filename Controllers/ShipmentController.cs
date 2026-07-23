using Microsoft.AspNetCore.Mvc;
using Liguria_Trasporti.Services;

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
}