using Liguria_Trasporti.Data;
using Liguria_Trasporti.Models;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Services;

public class ShipmentService
{
    private readonly AppDbContext _dbContext;
    public ShipmentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Shipment>> GetAllShipments()
    {
        return await _dbContext.Shipments.ToListAsync();
    }
}