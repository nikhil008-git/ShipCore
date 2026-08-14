using ShipCore.Reconciliation;

namespace ShipCore.Tests;

public sealed class InvoiceReconcilerTests
{
    [Fact]
    public void Aggregates_multi_parcel_lines_while_reporting_duplicate_and_missing_records()
    {
        var reconciler = new InvoiceReconciler();
        var report = reconciler.Reconcile(
        [
            new("multi", 5.00m, 400, "EU", "EUR"),
            new("multi", 5.00m, 400, "EU", "EUR"),
            new("carrier-only", 3m, 100, "EU", "EUR")
        ],
        [
            new("multi", 8m, 790, "EU", "EUR"),
            new("customer-only", 4m, 100, "EU", "EUR")
        ]);

        Assert.Contains(report.Discrepancies, x => x is { Type: "DuplicateCarrierBilling", TrackingNumber: "multi" });
        Assert.Contains(report.Discrepancies, x => x is { Type: "PriceMismatch", TrackingNumber: "multi", Magnitude: "+2.00" });
        Assert.DoesNotContain(report.Discrepancies, x => x is { Type: "WeightMismatch", TrackingNumber: "multi" });
        Assert.Contains(report.Discrepancies, x => x.Type == "MissingCarrierInvoice");
        Assert.Contains(report.Discrepancies, x => x.Type == "MissingCustomerCharge");
    }
}
