using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ShipCore.Reconciliation;

namespace ShipCore.Functions;

public sealed class MondayReconciliationFunction(InvoiceReconciler reconciler, ILogger<MondayReconciliationFunction> logger)
{
    [Function(nameof(MondayReconciliationFunction))]
    public void Run([TimerTrigger("0 0 6 * * 1")] TimerInfo timer)
    {
        // Invoice ingestion is intentionally stubbed. In production this function streams
        // partitioned input from blob/SQL, then persists each partition's idempotent run state.
        var report = reconciler.Reconcile([], []);
        logger.LogInformation("Monday reconciliation completed with {Count} discrepancies.", report.Count);
    }
}
