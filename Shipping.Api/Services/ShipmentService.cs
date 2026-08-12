using Microsoft.EntityFrameworkCore;
using ShipCore.Data;
using ShipCore.DTOs;
using ShipCore.Models;

namespace ShipCore.Services;

public class ShipmentService
{
    // access to the db 
    private readonly AppDbContext _db;

    //constructor ASP.NET dependency injection wil; give us AppDbContext
    public ShipmentService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<List<Shipment>> GetAllAsync(int userId)
    {
        return await _db.shipments
            .Where(s => s.UserId == userId)
            .Include(s.Carrier)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
    // getting shipments all user logged in.
    public async Task<Shipment?> GetByIdAsync(int id, int userId)
    {
        return await _db.Shipments
            .Include(s => s.Carrier)
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(
                s => s.Id == id && s.UserId == userId
            );
    }

    public async Task<Shipment> CreateAsync(
        CreateShipmentRequest request,
        int userId)
    {

        // obj store in memory
        var shipment = new Shipment
        {
            TrackingNumber =
                $"SHIP-{Guid.NewGuid().ToString()[..8].ToUpper()}",

            Destination = request.Destination,
            CarrierId = request.CarrierId,
            UserId = userId,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        // inserted row.
        _db.Shipments.Add(shipment);

        // tracking inserted.
        _db.TrackingEvents.Add(new TrackingEvent
        {
            Shipment = shipment,
            Status = ShipmentStatus.Created,
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return shipment;
    }
    public async Task<Shipment?> UpdateStatusAsync(
       int id,
       ShipmentStatus status,
       int userId)
    {
        var shipment = await _db.Shipments
            .FirstOrDefaultAsync(
                s => s.Id == id && s.UserId == userId
            );

        if (shipment is null)
        {
            return null;
        }

        shipment.Status = status;    //   { "status": "InTransit" }


        _db.TrackingEvents.Add(new TrackingEvent
        {
            ShipmentId = shipment.Id,
            Status = status,
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return shipment;
    }
    public async Task<List<TrackingEvent>> GetTrackingAsync(
int shipmentId,
int userId)
    {
        return await _db.TrackingEvents
            .Where(t =>
                t.ShipmentId == shipmentId &&
                t.Shipment.UserId == userId)
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
    }
    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var shipment = await _db.Shipments
            .FirstOrDefaultAsync(
                s => s.Id == id && s.UserId == userId
            );

        if (shipment is null)
        {
            return false;
        }

        _db.Shipments.Remove(shipment);

        await _db.SaveChangesAsync();

        return true;
    }
}