using Microsoft.EntityFrameworkCore;
using ShipCore.CarrierIntegrations;
using ShipCore.CarrierIntegrations.RapidPost;
using ShipCore.CarrierIntegrations.SpeedShip;
using ShipCore.Data;
using ShipCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<CarrierIntegrationOptions>(
    builder.Configuration.GetSection(CarrierIntegrationOptions.SectionName));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<SpeedShipStub>();
builder.Services.AddSingleton<ISpeedShipApi>(sp => sp.GetRequiredService<SpeedShipStub>());
builder.Services.AddSingleton<ITokenProvider>(sp => new TokenProvider(
    sp.GetRequiredService<ISpeedShipApi>().GetTokenAsync,
    sp.GetRequiredService<IClock>()));
builder.Services.AddSingleton<ICarrierIntegration, SpeedShipIntegration>();
builder.Services.AddSingleton<RapidPostStub>();
builder.Services.AddSingleton<IRapidPostApi>(sp => sp.GetRequiredService<RapidPostStub>());
builder.Services.AddSingleton<ICarrierIntegration, RapidPostIntegration>();
builder.Services.AddSingleton<ICarrierIntegrationResolver, CarrierIntegrationResolver>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
