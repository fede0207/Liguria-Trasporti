using System.Security.Claims;
using Liguria_Trasporti.Data;
using Liguria_Trasporti.DTOs;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Models;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Services;

public class ShipmentService(AppDbContext dbContext) : IShipmentService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<ServiceResult<IEnumerable<Shipment>>> GetAllShipments()
    {
        var shipments = await _dbContext.Shipments.ToListAsync();
        return ServiceResult<IEnumerable<Shipment>>.Ok(shipments);
    }

    public async Task<ServiceResult<ShipmentResponseDto>> GetShipmentById(Guid id)
    {
        var shipment = await _dbContext.Shipments.FindAsync(id);
        if (shipment is null)
        {
            return ServiceResult<ShipmentResponseDto>.NotFound(null!);
        }

        var response = new ShipmentResponseDto()
        {
            Id = shipment.Id,
            CustomerId = shipment.CustomerId,
            OriginAddressId = shipment.OriginAddressId,
            DestinationAddressId = shipment.DestinationAddressId,
            PlannedDeliveryDate = shipment.PlannedDeliveryDate,
            DeliveredAt = shipment.DeliveredAt,
            ProposedDriverId = shipment.ProposedDriverId,
            ProposedVehicleId = shipment.ProposedVehicleId,
            AssignedDriverId = shipment.AssignedDriverId,
            AssignedVehicleId = shipment.AssignedVehicleId,
            DriverRouteId = shipment.DriverRouteId,
            Note = shipment.Note,
            Priority = shipment.Priority,
            Status = shipment.Status
        };
        
        return ServiceResult<ShipmentResponseDto>.Ok(response);
    }
    
    public async Task<ServiceResult<ShipmentResponseDto>> CreateShipment(ShipmentRequestDto shipmentRequest, string userRole)
    {
        if (!CanCreateShipment(userRole))
        {
            return ServiceResult<ShipmentResponseDto>.Unauthorized(null!);
        }
        var shipment = new Shipment()
        {
            Id = Guid.NewGuid(),
            CustomerId = shipmentRequest.CustomerId,
            OriginAddressId = shipmentRequest.OriginAddressId,
            DestinationAddressId = shipmentRequest.DestinationAddressId,
            PlannedDeliveryDate = shipmentRequest.PlannedDeliveryDate,
            DeliveredAt = shipmentRequest.DeliveredAt,
            ProposedDriverId = shipmentRequest.ProposedDriverId,
            ProposedVehicleId = shipmentRequest.ProposedVehicleId,
            DriverRouteId = shipmentRequest.DriverRouteId,
            Note = shipmentRequest.Note,
            Priority = shipmentRequest.Priority,
            Status = shipmentRequest.Status
        };

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync();

        var response = new ShipmentResponseDto()
        {
            Id = shipment.Id,
            CustomerId = shipment.CustomerId,
            OriginAddressId = shipment.OriginAddressId,
            DestinationAddressId = shipment.DestinationAddressId,
            PlannedDeliveryDate = shipment.PlannedDeliveryDate,
            DeliveredAt = shipment.DeliveredAt,
            ProposedDriverId = shipment.ProposedDriverId,
            ProposedVehicleId = shipment.ProposedVehicleId,
            DriverRouteId = shipment.DriverRouteId,
            Note = shipment.Note,
            Priority = shipment.Priority,
            Status = shipment.Status
        };

        return ServiceResult<ShipmentResponseDto>.Ok(response);
    }

    public async Task<ServiceResult<ShipmentResponseDto>> ValidateShipment(Guid shipmentId, ValidateShipmentRequestDto shipmentRequest,
        string userRole)
    {
        if (!CanValidateShipment(userRole))
        {
            return ServiceResult<ShipmentResponseDto>.Unauthorized(null!);
        }
        
        var shipment = await _dbContext.Shipments.FindAsync(shipmentId);
        if (shipment is null)
        {
            return ServiceResult<ShipmentResponseDto>.NotFound(null!);
        }
        if (shipment.Status is not ShipmentStatus.InPlanning)
        {
            return ServiceResult<ShipmentResponseDto>.ValidationError();
        }

        if (!ValidateDriver(shipmentRequest.AssignedDriverId))
        {
            return ServiceResult<ShipmentResponseDto>.ValidationError();
        }

        if (!ValidateVehicle(shipmentRequest.AssignedVehicleId))
        {
            return ServiceResult<ShipmentResponseDto>.ValidationError();
        }
        
        var driver = await _dbContext.Employees.FindAsync(shipmentRequest.AssignedDriverId);
        if (driver is null || driver.Role is not EmployeeRole.Driver)
        {
            return ServiceResult<ShipmentResponseDto>.NotFound(null!);
        }
        
        shipment.AssignedDriverId = shipmentRequest.AssignedDriverId;
        shipment.AssignedVehicleId = shipmentRequest.AssignedVehicleId;
        shipment.Status = ShipmentStatus.Planned;
        
        await _dbContext.SaveChangesAsync();

        var response = new ShipmentResponseDto()
        {
            Id = shipment.Id,
            CustomerId = shipment.CustomerId,
            OriginAddressId = shipment.OriginAddressId,
            DestinationAddressId = shipment.DestinationAddressId,
            PlannedDeliveryDate = shipment.PlannedDeliveryDate,
            DeliveredAt = shipment.DeliveredAt,
            ProposedDriverId = shipment.ProposedDriverId,
            ProposedVehicleId = shipment.ProposedVehicleId,
            AssignedDriverId = shipment.AssignedDriverId,
            AssignedVehicleId = shipment.AssignedVehicleId,
            DriverRouteId = shipment.DriverRouteId,
            Note = shipment.Note,
            Priority = shipment.Priority,
            Status = shipment.Status
        };
        
        return ServiceResult<ShipmentResponseDto>.Ok(response);
    }
    
    private bool CanCreateShipment(string userRole)
    {
        if (Enum.TryParse<EmployeeRole>(userRole, true, out var role))
        {
            if (role is not EmployeeRole.EmployeeManager and not EmployeeRole.LogisticOperator)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    private bool CanValidateShipment(string userRole)
    {
        if (Enum.TryParse<EmployeeRole>(userRole, true, out var role))
        {
            if (role is not EmployeeRole.ShippingManager)
            {
                return false;
            }
            return true;
        }
        return false;
    }
    
    private bool ValidateDriver(Guid? driverId)
    {
        if (!driverId.HasValue || driverId == Guid.Empty)
        {
            return false;
        }
        return true;
    }

    private bool ValidateVehicle(Guid? vehicleId)
    {
        if (!vehicleId.HasValue || vehicleId == Guid.Empty)
        {
            return false;
        }
        return true;
    }
}

