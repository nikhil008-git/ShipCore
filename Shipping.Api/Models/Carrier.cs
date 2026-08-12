namespace ShipCore.Models;

public class Carrier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Shipment> Shipments { get; set; } = []; // similar fashion one to many carrier 1 : * shipments

}/* 
Id    Name

1     DHL
2     FedEx
3     UPS
*/