namespace ShipCore.Models;

public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ShipmentStatus Status { get; set; }
            = ShipmentStatus.Created;  // enum like created,    PickedUp, InTransit, Delivered

    // User relationship foriegn key many part.

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    // now // Carrier relationship

    public int CarrierId { get; set; }

    public Carrier Carrier { get; set; } = null!;

// now 1: many for tracking relationship, we ceerate a list. real quikc.
    public List<TrackingEvent> TrackingEvents { get; set; } = [];

}
