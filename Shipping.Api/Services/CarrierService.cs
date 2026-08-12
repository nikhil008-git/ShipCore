using Microsoft.EntityFrameworkCore;
using ShipCore.Data;
using ShipCore.Models;

namespace ShipCore.Services;

public class CarrierService
{
    private readonly AppDbContext _db;

    public CarrierService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Carrier>> GetAllAsync()
    {
        return await _db.Carriers
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Carrier> CreateAsync(string name)
    {
        var carrier = new Carrier
        {
            Name = name
        };

        _db.Carriers.Add(carrier);

        await _db.SaveChangesAsync();

        return carrier;
    }
}