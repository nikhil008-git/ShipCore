namespace ShipCore.Reconciliation;

// Pure O(n + m) reconciliation. Callers can partition input by a stable tracking-number
// hash and merge reports, avoiding a full weekly invoice in process memory.
public sealed class InvoiceReconciler(ReconciliationOptions? options = null)
{
    private readonly ReconciliationOptions _options = options ?? new ReconciliationOptions();

    public DiscrepancyReport Reconcile(
        IEnumerable<CarrierInvoiceLine> invoiceLines,
        IEnumerable<CustomerCharge> customerCharges)
    {
        var invoiceByTracking = invoiceLines.GroupBy(x => x.TrackingNumber, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToArray(), StringComparer.Ordinal);
        var chargesByTracking = customerCharges.GroupBy(x => x.TrackingNumber, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToArray(), StringComparer.Ordinal);
        var discrepancies = new List<Discrepancy>();

        foreach (var trackingNumber in invoiceByTracking.Keys.Union(chargesByTracking.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
        {
            var hasInvoice = invoiceByTracking.TryGetValue(trackingNumber, out var invoices);
            var hasCharge = chargesByTracking.TryGetValue(trackingNumber, out var charges);

            if (!hasInvoice)
            {
                var charge = AggregateCharges(charges!);
                discrepancies.Add(new("MissingCarrierInvoice", trackingNumber, "carrier invoice line", "none",
                    $"customer billed {charge.Amount:0.00}", charge.Currency));
                continue;
            }

            if (!hasCharge)
            {
                var invoice = AggregateInvoices(invoices!);
                discrepancies.Add(new("MissingCustomerCharge", trackingNumber, "customer charge", "none",
                    $"carrier invoiced {invoice.Amount:0.00}", invoice.Currency));
                AddDuplicateBilling(invoices!, discrepancies);
                continue;
            }

            var actual = AggregateInvoices(invoices!);
            var expected = AggregateCharges(charges!);
            AddDuplicateBilling(invoices!, discrepancies);

            // The initial supported mode is one currency per input. Keeping it on each
            // aggregate makes per-currency grouping/conversion an additive change later.
            if (!string.Equals(actual.Currency, expected.Currency, StringComparison.OrdinalIgnoreCase))
            {
                discrepancies.Add(new("CurrencyMismatch", trackingNumber, expected.Currency, actual.Currency, "currencies differ"));
                continue;
            }

            var amountDelta = actual.Amount - expected.Amount;
            if (Math.Abs(amountDelta) > _options.PriceTolerance)
                discrepancies.Add(new("PriceMismatch", trackingNumber, $"{expected.Amount:0.00}", $"{actual.Amount:0.00}",
                    $"{amountDelta:+0.00;-0.00;0.00}", actual.Currency));

            var weightDelta = actual.WeightGrams - expected.WeightGrams;
            if (Math.Abs(weightDelta) > _options.WeightToleranceGrams)
                discrepancies.Add(new("WeightMismatch", trackingNumber, $"{expected.WeightGrams}g", $"{actual.WeightGrams}g",
                    $"{weightDelta:+0;-0;0}g", actual.Currency));

            if (!string.Equals(actual.Zone, expected.Zone, StringComparison.OrdinalIgnoreCase))
                discrepancies.Add(new("ZoneMismatch", trackingNumber, expected.Zone, actual.Zone, "zone differs", actual.Currency));
        }

        return new DiscrepancyReport(discrepancies);
    }

    private static Aggregate AggregateInvoices(IEnumerable<CarrierInvoiceLine> lines) =>
        new(lines.Sum(x => x.Amount), lines.Sum(x => x.WeightGrams), SingleOrMixed(lines.Select(x => x.Zone)), SingleOrMixed(lines.Select(x => x.Currency)));

    private static Aggregate AggregateCharges(IEnumerable<CustomerCharge> charges) =>
        new(charges.Sum(x => x.BilledAmount), charges.Sum(x => x.DeclaredWeightGrams), SingleOrMixed(charges.Select(x => x.Zone)), SingleOrMixed(charges.Select(x => x.Currency)));

    private static string SingleOrMixed(IEnumerable<string> values)
    {
        var distinct = values.Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToArray();
        return distinct.Length == 1 ? distinct[0] : "MIXED";
    }

    private static void AddDuplicateBilling(IEnumerable<CarrierInvoiceLine> invoices, ICollection<Discrepancy> discrepancies)
    {
        foreach (var duplicate in invoices.GroupBy(x => new { x.Amount, x.WeightGrams, x.Zone, x.Currency })
                     .Where(x => x.Count() > 1))
        {
            var sample = duplicate.First();
            discrepancies.Add(new("DuplicateCarrierBilling", sample.TrackingNumber, "1 identical parcel line",
                $"{duplicate.Count()} identical parcel lines", $"{duplicate.Count() - 1} duplicate(s)", sample.Currency));
        }
    }

    private sealed record Aggregate(decimal Amount, int WeightGrams, string Zone, string Currency);
}
