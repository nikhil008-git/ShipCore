using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShipCore.Data;

#nullable disable

namespace ShipCore.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");
        modelBuilder.Entity("ShipCore.Data.ProcessedIntegrationRequest", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<string>("CarrierCode").HasMaxLength(50);
            b.Property<string>("CarrierTrackingNumber").HasMaxLength(200);
            b.Property<string>("ClientRef").HasMaxLength(200);
            b.Property<DateTimeOffset>("CreatedAtUtc");
            b.HasKey("Id");
            b.HasIndex("CarrierCode", "ClientRef").IsUnique();
            b.ToTable("ProcessedIntegrationRequests");
        });
        modelBuilder.Entity("ShipCore.Data.ReconciliationRun", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset?>("CompletedAtUtc");
            b.Property<DateOnly>("InvoiceWeek");
            b.Property<DateTimeOffset>("StartedAtUtc");
            b.Property<string>("Status");
            b.HasKey("Id");
            b.ToTable("ReconciliationRuns");
        });
    }
}
