using ShipCore.CarrierIntegrations;

namespace ShipCore.Tests;

public sealed class TokenProviderTests
{
    [Fact]
    public async Task Concurrent_cold_cache_calls_share_one_refresh_and_refresh_at_early_expiry()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        var releaseRefresh = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var refreshCalls = 0;
        var provider = new TokenProvider(async _ =>
        {
            Interlocked.Increment(ref refreshCalls);
            await releaseRefresh.Task;
            return new AccessToken("one-token", clock.UtcNow.AddMinutes(5));
        }, clock);

        var callers = Enumerable.Range(0, 50).Select(_ => provider.GetTokenAsync()).ToArray();
        Assert.Equal(1, Volatile.Read(ref refreshCalls));

        releaseRefresh.SetResult();
        var tokens = await Task.WhenAll(callers);

        Assert.All(tokens, token => Assert.Equal("one-token", token));
        Assert.Equal(1, refreshCalls);

        clock.Advance(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(31)));
        await provider.GetTokenAsync();
        Assert.Equal(2, refreshCalls);
    }
}
