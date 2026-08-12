namespace ShipCore.Models;

public class TrackingEvent
{
    public int Id { get; set; }

    public ShipmentStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
        = DateTime.UtcNow;

    // relationship of many with realtionship
    public int ShipmentId { get; set; }

    public Shipment Shipment { get; set; } = null!;
}

/*  this is how the thing gonna be
TrackingEvents

Id   ShipmentId    Status          Timestamp

1       42         Created         10:00
2       42         PickedUp        12:30
3       42         InTransit       15:00
4       42         Delivered       20:00*/