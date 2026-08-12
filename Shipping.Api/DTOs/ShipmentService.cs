// Now we put database/business logic here.
using Microsoft.EntityFrameworkCore;
using ShipCore.Data;
using ShipCore.DTOs;
using ShipCore.Models;

namespace ShipCore.Services;

public class ShipmentService
{
    private readonly AppDbContext _db;

// constructor
    public ShipmentService(AppDbContext db)
    {
        _db = db;
    }
}