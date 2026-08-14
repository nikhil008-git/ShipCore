# Shipping integration assessment

## Run

```bash
dotnet restore Shipping.Assessment.sln
dotnet test Shipping.Assessment.sln
dotnet run --project Shipping.Api.csproj
```

Create a simulated shipment with `POST /api/carriers/speedship/shipments`; use `rapidpost` to exercise the second implementation. The fake integrations are selected through DI and `CarrierIntegrations:TestMode` in `appsettings.json`.

## Design notes

- `ICarrierIntegration` is the extension seam. SpeedShip uses cached bearer-token authentication; RapidPost signs its different payload with HMAC. Neither carrier knows about the other.
- `TokenProvider` keeps token/expiry in an immutable record and shares one in-flight refresh task. It releases its lock before awaiting the network request, refreshes 30 seconds early, and removes a failed task so failures are never cached.
- Shipment creation carries a customer-controlled `clientRef`. The SpeedShip stub stores by that key before it simulates a lost response, making a retry return the same tracking number. A 401 invalidates the token and permits exactly one re-authentication attempt.
- The transient retry policy handles only timeout, 429 and 503 with exponential backoff plus jitter. It deliberately does not retry other 4xx responses. Labels return HTTP 202 while the stub's label is in its short eventual-consistency window; a client can poll with its own capped budget or await a webhook in production.
- Invoice reconciliation is pure and deterministic. It groups by tracking number, so matching is O(n + m) time and O(n + m) space for one partition. It assumes one currency per invoice; `MIXED` is surfaced as a currency discrepancy rather than converted silently.
- Duplicate billing is inferred from identical carrier lines because the supplied data has no parcel-line ID. A production feed should include an immutable carrier invoice-line/parcel ID to make this detection definitive.

## Scale and operations

For 1M+ records, stream invoice/customer data partitioned by a stable hash of tracking number, reconcile one partition at a time, and persist partition completion with its source checksum. The Monday Azure Function is timer-triggered; its input adapter is deliberately stubbed so the reconciliation core remains testable.

Azure Functions are at-least-once. I would use a unique `(invoiceWeek, partition, sourceChecksum)` row plus an outbox in the same EF Core transaction; duplicate invocations observe the completed row and have no second business effect. Give each carrier its own queue, concurrency cap, retry budget and circuit breaker, so a failing carrier cannot exhaust shared workers.

## Intentional scope cuts

The carrier APIs are deterministic in-memory stubs, not network clients. No currency conversion, real PDF rendering, webhook receiver, distributed token cache, or invoice-source adapter is implemented. In a multi-instance deployment, move the token/idempotency coordination to a shared store or accept per-instance token refresh while retaining carrier-side idempotency.
