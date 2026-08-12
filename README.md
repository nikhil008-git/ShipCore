# ShipCore

ShipCore is an ASP.NET Core API for user authentication, carriers, and shipment tracking.

## Requirements

- .NET 10 SDK

## Local setup

1. Create a local environment file at `Shipping.Api/.env`. This file is ignored by Git.

   ```sh
   export ConnectionStrings__DefaultConnection='Data Source=ShipCore.db'
   export Jwt__Key='replace-with-a-long-random-secret'
   ```

2. Load the variables and start the API.

   ```sh
   cd Shipping.Api
   source .env
   dotnet run
   ```

3. The API starts at `http://localhost:5190`. OpenAPI is available during development at `/openapi/v1.json`.

The SQLite database is created automatically on startup. Its local database files are ignored by Git.

## Configuration

Configuration uses standard ASP.NET Core environment variables. Double underscores map to nested configuration keys:

| Environment variable | Configuration key |
| --- | --- |
| `Jwt__Key` | `Jwt:Key` |
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |

`Program.cs` reads these values through `Environment.GetEnvironmentVariable`, which is the C# equivalent of accessing `process.env` in Node.js.

## API routes

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET, POST /api/carriers` (authenticated)
- `GET, POST /api/shipments` (authenticated)
- `GET, DELETE /api/shipments/{id}` (authenticated)
- `PATCH /api/shipments/{id}/status` (authenticated)
- `GET /api/shipments/{id}/tracking` (authenticated)
