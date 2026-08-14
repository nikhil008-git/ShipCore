using ShipCore.CarrierIntegrations;
using ShipCore.CarrierIntegrations.SpeedShip;

namespace ShipCore.Tests;

public sealed class SpeedShipIntegrationTests
{
    [Fact]
    public async Task Retry_after_lost_response_returns_the_original_idempotent_shipment()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        var stub = new SpeedShipStub(clock);
        var tokens = new TokenProvider(stub.GetTokenAsync, clock);
        var integration = new SpeedShipIntegration(stub, tokens, NoWaitRetryPolicy());
        stub.QueueShipmentFault(SpeedShipFault.TimeoutAfterCreate);

        var result = await integration.CreateShipmentAsync(new CarrierShipmentRequest("order-42", "Ada", "NL", 1.2m));

        Assert.Equal("SS-order-42", result.CarrierTrackingNumber);
        Assert.Equal(2, stub.ShipmentCreateRequests);
        Assert.Equal(1, stub.ShipmentCount);
    }

    [Fact]
    public async Task Retries_503_with_backoff_but_does_not_retry_bad_request()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        var stub = new SpeedShipStub(clock);
        var delays = new List<TimeSpan>();
        var integration = new SpeedShipIntegration(stub, new TokenProvider(stub.GetTokenAsync, clock), new RetryPolicy(new RetryPolicyOptions
        {
            MaxRetries = 2,
            BaseDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
            DelayAsync = (delay, _) => { delays.Add(delay); return Task.CompletedTask; }
        }));
        stub.QueueShipmentFault(SpeedShipFault.ServiceUnavailable);

        await integration.CreateShipmentAsync(new CarrierShipmentRequest("order-503", "Ada", "NL", 1m));

        Assert.Equal([TimeSpan.FromMilliseconds(10)], delays);
        Assert.Equal(2, stub.ShipmentCreateRequests);

        stub.QueueShipmentFault(SpeedShipFault.BadRequest);
        await Assert.ThrowsAsync<CarrierApiException>(() => integration.CreateShipmentAsync(
            new CarrierShipmentRequest("order-400", "Ada", "NL", 1m)));
        Assert.Equal(3, stub.ShipmentCreateRequests);
    }

    private static RetryPolicy NoWaitRetryPolicy() => new(new RetryPolicyOptions
    {
        MaxRetries = 2,
        UseJitter = false,
        DelayAsync = (_, _) => Task.CompletedTask
    });
}
