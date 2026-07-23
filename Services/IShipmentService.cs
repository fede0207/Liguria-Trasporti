using Liguria_Trasporti.Models;
using Microsoft.AspNetCore.Mvc;

namespace Liguria_Trasporti.Services;


public interface IShipmentService
{
    public Task<IEnumerable<Shipment>> GetAllShipments();
}