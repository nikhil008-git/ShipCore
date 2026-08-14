using Microsoft.Extensions.DependencyInjection;
using ShipCore.CarrierIntegrations.RapidPost;
using ShipCore.CarrierIntegrations.SpeedShip;
using ShipCore.Infrastructure;

namespace ShipCore.CarrierIntegrations;

public static class CarrierIntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddCarrierIntegrations(
        this IServiceCollection services,
        CarrierIntegrationOptions options)
    {
        if (!options.TestMode)
        {
            throw new InvalidOperationException(
                "No production carrier API clients are configured. Set CarrierIntegrations:TestMode to true " +
                "for the deterministic assessment stubs.");
        }

        services.AddSingleton<SpeedShipStub>();
        services.AddSingleton<ISpeedShipApi>(sp => sp.GetRequiredService<SpeedShipStub>());
        services.AddSingleton<ITokenProvider>(sp => new TokenProvider(
            sp.GetRequiredService<ISpeedShipApi>().GetTokenAsync,
            sp.GetRequiredService<IClock>()));
        services.AddSingleton<ICarrierIntegration, SpeedShipIntegration>();
        services.AddSingleton<RapidPostStub>();
        services.AddSingleton<IRapidPostApi>(sp => sp.GetRequiredService<RapidPostStub>());
        services.AddSingleton<ICarrierIntegration, RapidPostIntegration>();
        services.AddSingleton<ICarrierIntegrationResolver, CarrierIntegrationResolver>();
        return services;
    }
}
