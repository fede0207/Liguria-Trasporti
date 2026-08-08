using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Models;
namespace Liguria_Trasporti.Services;


public interface IShipmentService
{
    public Task<ServiceResult<IEnumerable<Shipment>>> GetAllShipments();
    public Task<ServiceResult<ShipmentResponseDto>> CreateShipment(ShipmentRequestDto shipmentRequest, string userRole);
    public Task<ServiceResult<ShipmentResponseDto>> GetShipmentById(Guid id);
}