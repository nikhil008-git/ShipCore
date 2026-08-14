using System.Security.Cryptography;
using System.Text;

namespace ShipCore.CarrierIntegrations.RapidPost;

public interface IRapidPostApi
{
    Task<ShipmentCreationResult> PostParcelAsync(string signature, RapidPostParcelPayload payload, CancellationToken cancellationToken);
    Task<LabelResult> DownloadDocumentAsync(string signature, string trackingNumber, CancellationToken cancellationToken);
}

public sealed record RapidPostParcelPayload(string ExternalReference, string Addressee, string CountryCode, int Grams);

public sealed class RapidPostIntegration(IRapidPostApi api, RetryPolicy? retryPolicy = null) : ICarrierIntegration
{
    private const string SharedSecret = "demo-rapidpost-secret";
    private readonly RetryPolicy _retryPolicy = retryPolicy ?? new RetryPolicy();
    public string CarrierCode => "rapidpost";

    public Task<ShipmentCreationResult> CreateShipmentAsync(CarrierShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new RapidPostParcelPayload(request.ClientRef, request.RecipientName, request.DestinationCountry,
            (int)Math.Ceiling(request.WeightKg * 1000));
        return _retryPolicy.ExecuteAsync(ct => api.PostParcelAsync(Sign(payload.ExternalReference), payload, ct), cancellationToken);
    }

    public Task<LabelResult> GetLabelAsync(string carrierTrackingNumber, CancellationToken cancellationToken = default) =>
        _retryPolicy.ExecuteAsync(ct => api.DownloadDocumentAsync(Sign(carrierTrackingNumber), carrierTrackingNumber, ct), cancellationToken);

    private static string Sign(string value) => Convert.ToHexString(HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(SharedSecret), Encoding.UTF8.GetBytes(value)));
}
