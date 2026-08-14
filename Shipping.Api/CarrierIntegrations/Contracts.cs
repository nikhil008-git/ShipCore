namespace ShipCore.CarrierIntegrations;

// This is the extension seam: each carrier maps its own authentication and payloads
// behind this contract, so adding a carrier never changes existing integrations.
public interface ICarrierIntegration
{
    string CarrierCode { get; }

    Task<ShipmentCreationResult> CreateShipmentAsync(
        CarrierShipmentRequest request,
        CancellationToken cancellationToken = default);

    Task<LabelResult> GetLabelAsync(
        string carrierTrackingNumber,
        CancellationToken cancellationToken = default);
}

public interface ICarrierIntegrationResolver
{
    ICarrierIntegration GetRequired(string carrierCode);
}

public sealed class CarrierIntegrationResolver(IEnumerable<ICarrierIntegration> integrations)
    : ICarrierIntegrationResolver
{
    private readonly IReadOnlyDictionary<string, ICarrierIntegration> _integrations = integrations
        .ToDictionary(x => x.CarrierCode, StringComparer.OrdinalIgnoreCase);

    public ICarrierIntegration GetRequired(string carrierCode) =>
        _integrations.TryGetValue(carrierCode, out var integration)
            ? integration
            : throw new KeyNotFoundException($"Unsupported carrier '{carrierCode}'.");
}

public sealed record CarrierShipmentRequest(
    string ClientRef,
    string RecipientName,
    string DestinationCountry,
    decimal WeightKg);

public sealed record ShipmentCreationResult(string CarrierTrackingNumber);

public sealed record LabelResult(string CarrierTrackingNumber, string LabelBase64, bool IsReady);
