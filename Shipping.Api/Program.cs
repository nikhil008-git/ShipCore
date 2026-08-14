using Microsoft.EntityFrameworkCore;
using ShipCore.CarrierIntegrations;
using ShipCore.CarrierIntegrations.RapidPost;
using ShipCore.CarrierIntegrations.SpeedShip;
using ShipCore.Data;
using ShipCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IClock, SystemClock>();
var carrierOptions = builder.Configuration
    .GetSection(CarrierIntegrationOptions.SectionName)
    .Get<CarrierIntegrationOptions>() ?? new CarrierIntegrationOptions();
builder.Services.AddCarrierIntegrations(carrierOptions);

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
