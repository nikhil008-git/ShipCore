namespace ShipCore.DTOs;

public class CreateShipmentRequest
{
    public string Destination { get; set; } = string.Empty;

    public int CarrierId { get; set; }
}

// frontend only share
/* 
{
  "destination": "Amsterdam",
  "carrierId": 1
}

backend decides
UserId          ← JWT
TrackingNumber  ← backend
Status          ← Created
CreatedAt       ← backend
*/