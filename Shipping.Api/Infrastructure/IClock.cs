namespace ShipCore.Infrastructure;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
