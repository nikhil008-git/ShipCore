namespace ShipCore.CarrierIntegrations;

public sealed class RetryPolicy
{
    private readonly RetryPolicyOptions _options;
    private readonly Random _random;

    public RetryPolicy(RetryPolicyOptions? options = null, Random? random = null)
    {
        _options = options ?? new RetryPolicyOptions();
        _random = random ?? Random.Shared;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception exception) when (IsTransient(exception) && attempt < _options.MaxRetries)
            {
                await _options.DelayAsync(GetDelay(attempt), cancellationToken);
            }
        }
    }

    public bool IsTransient(Exception exception) => exception is TimeoutException ||
        exception is CarrierApiException { StatusCode: 429 or 503 };

    public TimeSpan GetDelay(int attempt)
    {
        var exponentialMilliseconds = _options.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var jitterMilliseconds = _options.UseJitter ? _random.NextDouble() * _options.BaseDelay.TotalMilliseconds : 0;
        return TimeSpan.FromMilliseconds(exponentialMilliseconds + jitterMilliseconds);
    }
}

public sealed class RetryPolicyOptions
{
    public int MaxRetries { get; init; } = 3;
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(100);
    public bool UseJitter { get; init; } = true;
    public Func<TimeSpan, CancellationToken, Task> DelayAsync { get; init; } = Task.Delay;
}
