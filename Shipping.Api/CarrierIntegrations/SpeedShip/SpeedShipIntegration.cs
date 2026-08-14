namespace ShipCore.CarrierIntegrations.SpeedShip;

public sealed class SpeedShipIntegration(
    ISpeedShipApi api,
    ITokenProvider tokens,
    RetryPolicy? retryPolicy = null) : ICarrierIntegration
{
    private readonly RetryPolicy _retryPolicy = retryPolicy ?? new RetryPolicy();
    public string CarrierCode => "speedship";

    public Task<ShipmentCreationResult> CreateShipmentAsync(CarrierShipmentRequest request, CancellationToken cancellationToken = default) =>
        ExecuteWithOneReauthenticationAsync(
            token => _retryPolicy.ExecuteAsync(ct => api.CreateShipmentAsync(token, request, ct), cancellationToken), cancellationToken);

    public Task<LabelResult> GetLabelAsync(string carrierTrackingNumber, CancellationToken cancellationToken = default) =>
        ExecuteWithOneReauthenticationAsync(
            token => _retryPolicy.ExecuteAsync(ct => api.GetLabelAsync(token, carrierTrackingNumber, ct), cancellationToken), cancellationToken);

    private async Task<T> ExecuteWithOneReauthenticationAsync<T>(
        Func<string, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        for (var authenticationAttempt = 0; authenticationAttempt < 2; authenticationAttempt++)
        {
            var token = await tokens.GetTokenAsync(cancellationToken);
            try
            {
                return await operation(token);
            }
            catch (CarrierApiException exception) when (exception.StatusCode == 401 && authenticationAttempt == 0)
            {
                tokens.Invalidate(token);
            }
        }

        throw new InvalidOperationException("Unreachable.");
    }
}
