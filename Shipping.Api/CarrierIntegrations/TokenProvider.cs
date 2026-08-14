using ShipCore.Infrastructure;

namespace ShipCore.CarrierIntegrations;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    void Invalidate(string rejectedToken);
}

public sealed class TokenProvider : ITokenProvider
{
    private readonly Func<CancellationToken, Task<AccessToken>> _refresh;
    private readonly IClock _clock;
    private readonly TimeSpan _refreshMargin;
    private readonly object _sync = new();
    private AccessToken? _cachedToken;
    private Task<AccessToken>? _refreshInFlight;

    public TokenProvider(
        Func<CancellationToken, Task<AccessToken>> refresh,
        IClock clock,
        TimeSpan? refreshMargin = null)
    {
        _refresh = refresh;
        _clock = clock;
        _refreshMargin = refreshMargin ?? TimeSpan.FromSeconds(30);
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = Volatile.Read(ref _cachedToken);
        if (IsUsable(token)) return token!.Value;

        Task<AccessToken> refreshTask;
        TaskCompletionSource<AccessToken>? refreshCompletion = null;
        lock (_sync)
        {
            token = _cachedToken;
            if (IsUsable(token)) return token!.Value;
            if (_refreshInFlight is null)
            {
                refreshCompletion = new TaskCompletionSource<AccessToken>(TaskCreationOptions.RunContinuationsAsynchronously);
                _refreshInFlight = refreshCompletion.Task;
            }
            refreshTask = _refreshInFlight;
        }

        if (refreshCompletion is not null)
            _ = CompleteRefreshAsync(refreshCompletion);

        // The lock is released before any network await; concurrent callers share this task.
        return (await refreshTask.WaitAsync(cancellationToken)).Value;
    }

    public void Invalidate(string rejectedToken)
    {
        lock (_sync)
        {
            if (_cachedToken?.Value == rejectedToken)
                _cachedToken = null;
        }
    }

    private bool IsUsable(AccessToken? token) =>
        token is not null && token.ExpiresAt - _refreshMargin > _clock.UtcNow;

    private async Task CompleteRefreshAsync(TaskCompletionSource<AccessToken> completion)
    {
        try
        {
            var token = await _refresh(CancellationToken.None);
            Volatile.Write(ref _cachedToken, token);
            completion.TrySetResult(token);
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
        finally
        {
            lock (_sync)
            {
                // A failed refresh is deliberately removed too, so the next caller retries.
                if (ReferenceEquals(_refreshInFlight, completion.Task))
                    _refreshInFlight = null;
            }
        }
    }
}
