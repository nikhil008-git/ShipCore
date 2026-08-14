using System.Collections.Concurrent;
using ShipCore.Infrastructure;

namespace ShipCore.CarrierIntegrations.SpeedShip;

public interface ISpeedShipApi
{
    Task<AccessToken> GetTokenAsync(CancellationToken cancellationToken);
    Task<ShipmentCreationResult> CreateShipmentAsync(string bearerToken, CarrierShipmentRequest request, CancellationToken cancellationToken);
    Task<LabelResult> GetLabelAsync(string bearerToken, string trackingNumber, CancellationToken cancellationToken);
}

public enum SpeedShipFault { Unauthorized, BadRequest, TooManyRequests, ServiceUnavailable, Timeout, TimeoutAfterCreate }

public sealed class SpeedShipStub(IClock clock) : ISpeedShipApi
{
    private readonly ConcurrentDictionary<string, StoredShipment> _shipmentsByClientRef = new();
    private readonly ConcurrentDictionary<string, AccessToken> _issuedTokens = new();
    private readonly ConcurrentQueue<SpeedShipFault> _shipmentFaults = new();
    private readonly IClock _clock = clock;
    private int _tokenRequests;
    private int _shipmentCreateRequests;

    public int TokenRequests => Volatile.Read(ref _tokenRequests);
    public int ShipmentCreateRequests => Volatile.Read(ref _shipmentCreateRequests);
    public int ShipmentCount => _shipmentsByClientRef.Count;
    public void QueueShipmentFault(SpeedShipFault fault) => _shipmentFaults.Enqueue(fault);

    public Task<AccessToken> GetTokenAsync(CancellationToken cancellationToken)
    {
        var number = Interlocked.Increment(ref _tokenRequests);
        var token = new AccessToken($"speedship-token-{number}", _clock.UtcNow.AddMinutes(10));
        _issuedTokens[token.Value] = token;
        return Task.FromResult(token);
    }

    public Task<ShipmentCreationResult> CreateShipmentAsync(string bearerToken, CarrierShipmentRequest request, CancellationToken cancellationToken)
    {
        ValidateToken(bearerToken);
        Interlocked.Increment(ref _shipmentCreateRequests);
        var shipment = _shipmentsByClientRef.GetOrAdd(request.ClientRef, key =>
            new StoredShipment($"SS-{key}", _clock.UtcNow.AddSeconds(2)));

        if (_shipmentFaults.TryDequeue(out var fault)) ThrowFault(fault);
        return Task.FromResult(new ShipmentCreationResult(shipment.TrackingNumber));
    }

    public Task<LabelResult> GetLabelAsync(string bearerToken, string trackingNumber, CancellationToken cancellationToken)
    {
        ValidateToken(bearerToken);
        var shipment = _shipmentsByClientRef.Values.SingleOrDefault(x => x.TrackingNumber == trackingNumber)
            ?? throw new CarrierApiException(404, "Unknown tracking number.");
        if (shipment.LabelAvailableAt > _clock.UtcNow)
            throw new CarrierApiException(404, "Label is not ready yet.");

        return Task.FromResult(new LabelResult(trackingNumber,
            Convert.ToBase64String("%PDF-1.4 fake SpeedShip label"u8.ToArray()), true));
    }

    private void ValidateToken(string token)
    {
        if (!_issuedTokens.TryGetValue(token, out var issued) || issued.ExpiresAt <= _clock.UtcNow)
            throw new CarrierApiException(401, "Invalid bearer token.");
    }

    private static void ThrowFault(SpeedShipFault fault) => throw fault switch
    {
        SpeedShipFault.Unauthorized => new CarrierApiException(401, "Token revoked mid-flight."),
        SpeedShipFault.BadRequest => new CarrierApiException(400, "Invalid shipment payload."),
        SpeedShipFault.TooManyRequests => new CarrierApiException(429, "Rate limited."),
        SpeedShipFault.ServiceUnavailable => new CarrierApiException(503, "Temporary outage."),
        SpeedShipFault.Timeout or SpeedShipFault.TimeoutAfterCreate => new TimeoutException("Simulated timeout."),
        _ => throw new InvalidOperationException("Unknown stub fault.")
    };

    private sealed record StoredShipment(string TrackingNumber, DateTimeOffset LabelAvailableAt);
}
