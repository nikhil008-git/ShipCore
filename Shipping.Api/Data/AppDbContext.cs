using Microsoft.EntityFrameworkCore;
using ShipCore.Models;

namespace ShipCore.Data;

// Creates your custom DB client by extending EF Core’s built-in DbContext.

public class AppDbContext : DbContext
{
    // this below is simply contructor. if .NET will give our DbContext configuration such as:
    public AppDbContext(
         DbContextOptions<AppDbContext> options
     ) : base(options)
    {
    }
    public DbSet<User> Users { get; set; } //EF Core, I want access to the Shipments table.

    public DbSet<Shipment> Shipments { get; set; }

    public DbSet<Carrier> Carriers { get; set; }

    public DbSet<TrackingEvent> TrackingEvents { get; set; }


}