namespace ShipCore.CarrierIntegrations;

public sealed class CarrierApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
