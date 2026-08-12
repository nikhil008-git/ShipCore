using ShipCore.Models;

namespace ShipCore.DTOs;

public class UpdateShipmentStatusRequest
{
    public ShipmentStatus Status { get; set; }
}

/*

{
  "status": "Delivered"
}

postman with put/patch

PATCH /api/shipments/4/status
*/