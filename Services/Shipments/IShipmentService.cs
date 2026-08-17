using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Models;
namespace Liguria_Trasporti.Services.Shipments;


public interface IShipmentService
{
    public Task<ServiceResult<IEnumerable<ShipmentResponseDto>>> GetAllShipments();
    public Task<ServiceResult<ShipmentResponseDto>> CreateShipment(ShipmentRequestDto shipmentRequest);
    public Task<ServiceResult<ShipmentResponseDto>> GetShipmentById(Guid id);

    public Task<ServiceResult<ShipmentResponseDto>> ValidateShipment(Guid shipmentId,
        ValidateShipmentRequestDto shipmentRequest);
}