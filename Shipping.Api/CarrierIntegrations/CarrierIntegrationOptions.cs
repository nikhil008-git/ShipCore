namespace ShipCore.CarrierIntegrations;

public sealed class CarrierIntegrationOptions
{
    public const string SectionName = "CarrierIntegrations";

    // Registered once through DI. Test doubles read this configuration rather than
    // scattering test-mode conditionals through carrier code.
    public bool TestMode { get; init; }
}
