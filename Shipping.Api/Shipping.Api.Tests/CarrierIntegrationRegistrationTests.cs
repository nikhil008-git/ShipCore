using Microsoft.Extensions.DependencyInjection;
using ShipCore.CarrierIntegrations;
using ShipCore.CarrierIntegrations.RapidPost;
using ShipCore.CarrierIntegrations.SpeedShip;
using ShipCore.Infrastructure;

namespace ShipCore.Tests;

public sealed class CarrierIntegrationRegistrationTests
{
    [Fact]
    public void Test_mode_registers_only_the_deterministic_test_clients()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock>(new TestClock(DateTimeOffset.UtcNow));

        services.AddCarrierIntegrations(new CarrierIntegrationOptions { TestMode = true });

        using var provider = services.BuildServiceProvider();
        Assert.IsType<SpeedShipStub>(provider.GetRequiredService<ISpeedShipApi>());
        Assert.IsType<RapidPostStub>(provider.GetRequiredService<IRapidPostApi>());
    }

    [Fact]
    public void Non_test_mode_requires_production_client_configuration()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddCarrierIntegrations(new CarrierIntegrationOptions { TestMode = false }));

        Assert.Contains("No production carrier API clients", exception.Message);
    }
}
