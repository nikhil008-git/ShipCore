using ShipCore.Infrastructure;

namespace ShipCore.Tests;

public sealed class TestClock(DateTimeOffset initial) : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = initial;
    public void Advance(TimeSpan duration) => UtcNow = UtcNow.Add(duration);
}
