namespace ShipCore.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public List<Shipment> Shipments { get; set; } = []; // this 1 to many. like Nick can own shipment 1, 2, 3, etc. 
}