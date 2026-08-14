using Microsoft.EntityFrameworkCore;

namespace ShipCore.Data;

// Only operational state belongs here; carrier payloads and reconciliation remain
// independently testable domain code.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedIntegrationRequest> ProcessedIntegrationRequests => Set<ProcessedIntegrationRequest>();
    public DbSet<ReconciliationRun> ReconciliationRuns => Set<ReconciliationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedIntegrationRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.CarrierCode, x.ClientRef }).IsUnique();
            entity.Property(x => x.CarrierCode).HasMaxLength(50);
            entity.Property(x => x.ClientRef).HasMaxLength(200);
            entity.Property(x => x.CarrierTrackingNumber).HasMaxLength(200);
        });
        modelBuilder.Entity<ReconciliationRun>(entity => entity.HasKey(x => x.Id));
    }
}

public sealed class ProcessedIntegrationRequest
{
    public Guid Id { get; set; }
    public string CarrierCode { get; set; } = string.Empty;
    public string ClientRef { get; set; } = string.Empty;
    public string CarrierTrackingNumber { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ReconciliationRun
{
    public Guid Id { get; set; }
    public DateOnly InvoiceWeek { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}
