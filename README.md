# ShipCore shipping integration assessment

This repository implements the shipping-integration assessment with deterministic in-memory carrier stubs, a reusable carrier abstraction, a thread-safe token cache, and pure invoice-reconciliation logic.

## Run locally

Prerequisite: .NET SDK 10.

```bash
cd Shipping.Api
dotnet restore Shipping.Assessment.sln
dotnet test Shipping.Assessment.sln
dotnet run --project Shipping.Api.csproj
```

The API runs at `http://localhost:5190` in the default launch profile. SQLite migrations are applied automatically on startup.

`CarrierIntegrations:TestMode` is `true` in `Shipping.Api/appsettings.json`. It selects deterministic, side-effect-free carrier stubs through dependency injection. Setting it to `false` intentionally fails startup because production HTTP clients are outside this assessment's scope; the application must not silently run test doubles in production.

## API smoke tests

Create a SpeedShip shipment:

```http
POST /api/carriers/speedship/shipments
Content-Type: application/json

{
  "clientRef": "ORDER-1001",
  "recipientName": "Ada Lovelace",
  "destinationCountry": "NL",
  "weightKg": 1.25
}
```

It returns a tracking number such as `SS-ORDER-1001`. Repeating the same request returns the same tracking number: `clientRef` is the idempotency key.

Fetch the label:

```http
GET /api/carriers/speedship/shipments/SS-ORDER-1001/label
```

SpeedShip labels are eventually consistent. The API returns `202 Accepted` with `{ "status": "pending" }` during the short availability window. Retry with a short, capped polling interval; when ready it returns the base64 PDF label with `200 OK`.

RapidPost uses the same calling API but a different internal contract:

```http
POST /api/carriers/rapidpost/shipments
Content-Type: application/json

{
  "clientRef": "ORDER-2001",
  "recipientName": "Ada Lovelace",
  "destinationCountry": "NL",
  "weightKg": 0.75
}
```

`GET /api/carriers/rapidpost/shipments/RP-ORDER-2001/label` returns its label immediately.

Invoice reconciliation is deliberately a pure C# service, not an HTTP endpoint. The scheduled Azure Function owns orchestration; production ingestion/persistence is stubbed so the reconciliation algorithm remains deterministic and trivial to test.

## Design notes

### Carrier extension seam

`ICarrierIntegration` is the extension seam. The controller resolves a carrier by code and only knows the shared request/result contracts. `SpeedShipIntegration` implements cached bearer-token authentication; `RapidPostIntegration` maps the same request into its own payload and signs it with HMAC. Adding a carrier does not require changing either existing carrier or the controller.

### Authentication, idempotency, and retries

- `TokenProvider` stores token and expiry together in an immutable record, uses a 30-second early-refresh margin, and shares one in-flight refresh task. Fifty callers with a cold cache result in one token request; the lock is released before awaiting the refresh. Failed refreshes are removed rather than cached.
- SpeedShip stores a shipment by `clientRef` before it can simulate a lost response. A retry returns the original carrier tracking number rather than creating another shipment.
- A SpeedShip `401` invalidates the rejected token and retries authentication exactly once. The bounded loop prevents an infinite auth loop.
- `RetryPolicy` retries only timeouts, `429`, and `503`, using exponential backoff plus jitter. It does not retry ordinary `4xx` responses. Retrying shipment creation is safe because the carrier honours the supplied idempotency key.
- For labels, the caller gets `202` while the label is pending. A client may poll with a capped budget; in production, a webhook/outbox flow is preferable for longer delays.

### Reconciliation

`InvoiceReconciler` is side-effect-free and deterministic. It groups both inputs by tracking number, aggregates multi-parcel lines, and emits a structured `DiscrepancyReport` containing discrepancy type, tracking number, expected value, actual value, magnitude, and currency where applicable.

It reports missing carrier invoices, missing customer charges, price mismatches, weight mismatches, zone mismatches, currency mismatches, and duplicate carrier billing. Price and weight tolerances are explicit and configurable through `ReconciliationOptions` (default: €0.01 and 10 g). The current assumption is one currency per invoice/input group; mixed values are surfaced as `MIXED` rather than silently converted.

Matching is `O(n + m)` time and `O(n + m)` memory for one partition, where `n` is carrier invoice lines and `m` is customer charges. Duplicate billing is inferred from identical carrier lines because the supplied model has no immutable parcel/invoice-line ID; production data should provide that ID for definitive duplicate detection.

### Scale and Azure Functions

For 1M+ weekly records, an ingestion adapter should stream both data sets partitioned by a stable hash of tracking number, reconcile one partition at a time, and persist each partition's source checksum and completion state. This avoids loading a full invoice week into memory and permits safe restarts.

Azure Functions timer triggers are at-least-once. To give reconciliation exactly-once business effects, persist a unique `(invoiceWeek, partition, sourceChecksum)` record and an outbox entry in the same transaction. A duplicate invocation observes the completed partition and produces no second effect.

Each carrier should have an independent queue, concurrency cap, retry budget, and circuit breaker. This keeps a failing carrier from consuming shared worker capacity or taking down the other integrations.

## Tests

The tests focus on failure and concurrency cases:

- 50 concurrent cold-cache token callers share one refresh and refresh at the early-expiry boundary.
- A failed token refresh is not cached.
- A timeout after SpeedShip creates a shipment retries safely and returns the original tracking number.
- `503` is retried with backoff; `400` is not.
- A mid-flight `401` invalidates the token and performs one re-authentication retry.
- Reconciliation verifies multi-parcel aggregation, duplicate detection, tolerances, and missing records.

```bash
cd Shipping.Api
dotnet test Shipping.Assessment.sln
```

## Docker

The Dockerfile restores dependencies, runs tests during the build, publishes the API, and runs the published service:

```bash
cd Shipping.Api
docker build -t shipcore-shipping-api .
docker run --rm -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 shipcore-shipping-api
```

## Deliberate scope cuts / next steps

The carrier APIs are in-memory test stubs rather than real HTTP clients. Real PDF generation, webhook handling, distributed token/idempotency coordination, currency conversion, and the invoice-source adapter are not implemented. In a multi-instance deployment, move token and idempotency coordination to shared infrastructure while retaining carrier-side idempotency guarantees.

With more time, add per-carrier configured HTTP clients, contract tests against a test server, durable invoice ingestion, an outbox-backed webhook flow, and metrics for retries, token refreshes, reconciliation partitions, and carrier circuit breakers.
