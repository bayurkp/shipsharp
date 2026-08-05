# ShipSharp API

> Sea freight shipping management REST API built with ASP.NET Core 8 and Clean Architecture.

---

## Overview

ShipSharp is a backend API for managing sea freight logistics operations. It allows operators and administrators to manage shipments, assign vessels, track cargo status, and provide public shipment tracking for customers.

**Key capabilities:**

- JWT-based authentication with role-based access control (Admin / Operator)
- Full shipment lifecycle management with enforced status progression
- Immutable audit trail for all status changes
- Public shipment tracking endpoint (no auth required)
- Master data management for ports and vessels
- Customer profile management with soft delete

---

## Tech Stack

| Category         | Technology                      |
| ---------------- | ------------------------------- |
| Framework        | .NET 8.0 — ASP.NET Core Web API |
| ORM              | Entity Framework Core 8.0       |
| Database         | Microsoft SQL Server            |
| Validation       | FluentValidation 11.x           |
| Authentication   | JWT Bearer                      |
| Password Hashing | BCrypt.Net-Next                 |
| Logging          | Serilog                         |
| API Docs         | Scalar (at `/docs`)             |
| Testing          | xUnit + Moq + FluentAssertions  |

---

## Project Structure

```
ShipSharp/
├── src/
│   ├── Domain/           # Entities, enums, repository interfaces
│   ├── Application/      # Services, DTOs, validators (feature-based)
│   ├── Infrastructure/   # EF Core, repositories, JWT/BCrypt services
│   └── API/              # Controllers, middleware, Program.cs
├── tests/
│   └── ShipSharp.Tests/  # Unit and integration tests
├── docs/
│   └── prds/
│       └── prd-main.md   # Full product requirements
├── AGENTS.md             # AI agent instructions
└── ShipSharp.sln
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or LocalDB for development)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Setup

**1. Clone the repository**

```bash
git clone https://github.com/your-username/shipsharp.git
cd shipsharp
```

**2. Configure the connection string**

Edit `src/API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShipSharpDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "ShipSharp",
    "Audience": "ShipSharpClient",
    "ExpirationInMinutes": 60
  }
}
```

**3. Apply migrations and seed data**

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

**4. Run the API**

```bash
dotnet run --project src/API
```

**5. Open API documentation**

Navigate to `https://localhost:{port}/docs` to view the interactive Scalar API reference.

---

## API Reference

All responses follow the **ByJSON** envelope format:

```json
{
  "data": {},
  "error": null,
  "meta": {
    "request_id": "uuid",
    "timestamp": "2026-08-05T15:00:00Z"
  }
}
```

### Authentication

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:

```json
{
  "data": {
    "access_token": "eyJ...",
    "token_type": "Bearer",
    "expires_in": 3600,
    "user": {
      "id": "...",
      "username": "admin",
      "full_name": "System Administrator",
      "role": "Admin"
    }
  },
  "error": null,
  "meta": { "request_id": "...", "timestamp": "..." }
}
```

### Shipment Tracking (Public)

```http
GET /api/shipments/track/{tracking_number}
```

No authentication required.

Response:

```json
{
  "data": {
    "tracking_number": "SHP-20260001",
    "current_status": "At Sea",
    "estimated_arrival": "2026-09-01T08:00:00Z",
    "history": [
      {
        "status": "Booked",
        "timestamp": "2026-08-01T10:00:00Z",
        "updated_by": "operator1"
      },
      {
        "status": "Loading",
        "timestamp": "2026-08-03T08:00:00Z",
        "updated_by": "operator1"
      },
      {
        "status": "At Sea",
        "timestamp": "2026-08-05T06:00:00Z",
        "updated_by": "operator2"
      }
    ]
  },
  "error": null,
  "meta": { "request_id": "...", "timestamp": "..." }
}
```

### Shipment Status Lifecycle

```
Booked → Loading → Departed → At Sea → Arrived → Delivered
```

- Transitions must be sequential — no skipping, no reversal.
- `Delivered` shipments are permanently immutable.
- Every transition is logged with operator username and UTC timestamp.

---

## Key Endpoints

| Method   | Endpoint                        | Auth           | Description          |
| -------- | ------------------------------- | -------------- | -------------------- |
| `POST`   | `/api/auth/login`               | Public         | Login                |
| `GET`    | `/api/customers`                | Admin/Operator | List customers       |
| `POST`   | `/api/customers`                | Admin/Operator | Create customer      |
| `PUT`    | `/api/customers/{id}`           | Admin/Operator | Update customer      |
| `DELETE` | `/api/customers/{id}`           | Admin          | Soft-delete customer |
| `GET`    | `/api/ports`                    | Admin/Operator | List ports           |
| `POST`   | `/api/ports`                    | Admin          | Create port          |
| `GET`    | `/api/vessels`                  | Admin/Operator | List vessels         |
| `POST`   | `/api/vessels`                  | Admin          | Create vessel        |
| `POST`   | `/api/vessels/{id}/activate`    | Admin          | Activate vessel      |
| `POST`   | `/api/vessels/{id}/deactivate`  | Admin          | Deactivate vessel    |
| `GET`    | `/api/shipments`                | Admin/Operator | List shipments       |
| `POST`   | `/api/shipments`                | Admin/Operator | Create shipment      |
| `GET`    | `/api/shipments/{id}`           | Admin/Operator | Get shipment         |
| `PUT`    | `/api/shipments/{id}`           | Admin/Operator | Update shipment      |
| `PATCH`  | `/api/shipments/{id}/status`    | Admin/Operator | Advance status       |
| `GET`    | `/api/shipments/track/{number}` | **Public**     | Track shipment       |

Full interactive docs available at `/docs` when running in development.

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```

Integration tests use SQLite in-memory database via `WebApplicationFactory` — no SQL Server required.

---

## Default Seed Data

The seeder (`AppDbSeeder`) creates the following on first run:

**Users:**
| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `operator` | `Operator@123` | Operator |

**Ports:** Surabaya (SUB), Jakarta (JKT), Singapore (SIN), Port Klang (PKL)

**Vessels:** MV ShipSharp One (active), MV ShipSharp Two (active)

---

## Response Format

This API follows the [ByJSON](https://github.com/bayu-dev/by-json) specification for all request and response structures:

- All JSON keys use `snake_case`
- All responses have `{ data, error, meta }` root structure
- Pagination info is in `meta.pagination`
- Validation errors include field-level `details` array

---

## License

MIT
