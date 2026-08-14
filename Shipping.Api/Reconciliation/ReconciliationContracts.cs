namespace ShipCore.Reconciliation;

public sealed record CarrierInvoiceLine(
    string TrackingNumber,
    decimal Amount,
    int WeightGrams,
    string Zone,
    string Currency);

public sealed record CustomerCharge(
    string TrackingNumber,
    decimal BilledAmount,
    int DeclaredWeightGrams,
    string Zone,
    string Currency);

public sealed class ReconciliationOptions
{
    public decimal PriceTolerance { get; init; } = 0.01m;
    public int WeightToleranceGrams { get; init; } = 10;
}

public sealed record Discrepancy(
    string Type,
    string TrackingNumber,
    string Expected,
    string Actual,
    string Magnitude,
    string? Currency = null);

public sealed record DiscrepancyReport(IReadOnlyList<Discrepancy> Discrepancies)
{
    public int Count => Discrepancies.Count;
}
