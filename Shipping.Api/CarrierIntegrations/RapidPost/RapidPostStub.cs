using System.Collections.Concurrent;

namespace ShipCore.CarrierIntegrations.RapidPost;

public sealed class RapidPostStub : IRapidPostApi
{
    private readonly ConcurrentDictionary<string, string> _trackingByReference = new();

    public Task<ShipmentCreationResult> PostParcelAsync(string signature, RapidPostParcelPayload payload, CancellationToken cancellationToken)
    {
        var tracking = _trackingByReference.GetOrAdd(payload.ExternalReference, key => $"RP-{key}");
        return Task.FromResult(new ShipmentCreationResult(tracking));
    }

    public Task<LabelResult> DownloadDocumentAsync(string signature, string trackingNumber, CancellationToken cancellationToken) =>
        Task.FromResult(new LabelResult(trackingNumber,
            Convert.ToBase64String("%PDF-1.4 fake RapidPost label"u8.ToArray()), true));
}
