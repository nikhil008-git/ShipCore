namespace ShipCore.Models;

public enum ShipmentStatus
{
    Created,
    LabelGenerated,
    PickedUp,
    InTransit,
    OutForDelivery,
    Delivered,
    Failed,
    Cancelled
}

/*  sort of type smthg.
type ShipmentStatus =
    | "Created"
    | "InTransit"
    | "Delivered";*/