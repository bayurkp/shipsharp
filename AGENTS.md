# AGENTS.md — ShipSharp API

This file provides context and instructions for AI coding agents (e.g., Antigravity, Copilot, Claude) working on the ShipSharp project.

---

## Project Overview

ShipSharp is a sea freight **Shipping Management REST API** built with **ASP.NET Core 8** and **Entity Framework Core 8**. It manages shipments, vessels, ports, and customers, enforcing strict business rules around shipment status transitions and audit logging.

Full requirements: [`docs/prds/prd-main.md`](./docs/prds/prd-main.md)

---

## Architecture

**Layer-First Clean Architecture** with **feature-based folder organization** inside each layer.

```
Domain ← Application ← Infrastructure
                 ↑
               API (entry point)
```

| Project Folder           | Assembly Name              | Role                                                                                          |
| ------------------------ | -------------------------- | --------------------------------------------------------------------------------------------- |
| `src/Domain/`            | `ShipSharp.Domain`         | Entities, enums, repository interfaces. Zero external deps.                                   |
| `src/Application/`       | `ShipSharp.Application`    | Services, DTOs, validators, service interfaces. Depends on Domain only.                       |
| `src/Infrastructure/`    | `ShipSharp.Infrastructure` | EF Core DbContext, repository impls, EF configs, JWT/BCrypt services. Depends on Application. |
| `src/API/`               | `ShipSharp.API`            | Controllers, middleware, Program.cs. Depends on Application + Infrastructure.                 |
| `tests/ShipSharp.Tests/` | `ShipSharp.Tests`          | xUnit tests (unit + integration).                                                             |

### Folder Organization (inside each layer)

Folders are organized **by feature** (not by type):

```
src/Application/
├── Common/           ← shared models, exceptions, interfaces
├── Auth/             ← AuthService, DTOs, Validators
├── Customers/        ← CustomerService, DTOs, Validators
├── Vessels/          ← VesselService, DTOs, Validators
├── Ports/            ← PortService, DTOs, Validators
├── Shipments/        ← ShipmentService, DTOs, Validators
└── DependencyInjection.cs
```

The same feature-based pattern applies to `Domain/`, `Infrastructure/`, and `API/`.

---

## Technology Stack

| Category            | Technology                                                          |
| ------------------- | ------------------------------------------------------------------- |
| Framework           | .NET 8.0 ASP.NET Core Web API                                       |
| ORM                 | Entity Framework Core 8.0                                           |
| Database            | SQL Server (dev: LocalDB or Docker)                                 |
| Validation          | FluentValidation 11.x                                               |
| Auth                | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)        |
| Password            | BCrypt.Net-Next                                                     |
| Logging             | Serilog (Console + Rolling File)                                    |
| API Docs            | Scalar (`Scalar.AspNetCore`) at `/docs` — **NOT Swashbuckle**       |
| Testing             | xUnit + Moq + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing` |
| Integration Test DB | SQLite In-Memory (`Microsoft.EntityFrameworkCore.Sqlite`)           |

---

## Key Business Rules

- Shipment status transitions MUST be sequential: `Booked → Loading → Departed → At Sea → Arrived → Delivered`
- No skipping steps, no reversal.
- `Delivered` shipments are **fully immutable** — reject any further changes.
- Every status change appends a record to `ShipmentStatusHistory`.
- Tracking numbers: `SHP-YYYYNNNN`, sequential per calendar year.
- Origin port ≠ Destination port.
- Only active vessels may be assigned to shipments.

---

## Response Format (ByJSON)

All responses MUST follow ByJSON: `{ data, error, meta }`.

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

**Do NOT use** `{ success, message, data, errors }` format.

See full spec: [ByJSON README](https://github.com/bayu-dev/by-json)

---

## Naming Conventions

| Item                 | Rule                                                |
| -------------------- | --------------------------------------------------- |
| JSON keys            | `snake_case`                                        |
| C# classes / methods | `PascalCase`                                        |
| Interfaces           | `I` prefix (e.g., `IShipmentRepository`)            |
| DTOs                 | `...Request` / `...Response` suffix                 |
| Validators           | `...Validator` suffix                               |
| Controllers          | Plural + `Controller` (e.g., `ShipmentsController`) |
| EF Configs           | `...Configuration` (e.g., `ShipmentConfiguration`)  |
| Resource URLs        | Plural, kebab-case (e.g., `/api/shipments`)         |

---

## Dependency Injection

Each layer exposes an extension method registered in `Program.cs`:

```csharp
// Program.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

- `AddApplication()` → defined in `src/Application/DependencyInjection.cs`
- `AddInfrastructure()` → defined in `src/Infrastructure/DependencyInjection.cs`

---

## Git Workflow

- Commits in **English**, using **Conventional Commits** format.
- Commit after each meaningful unit of work (per feature, per layer).

```
feat(domain): add Shipment entity and ShipmentStatus enum
feat(application): add ShipmentService with status transition logic
fix(api): return 422 instead of 400 for business rule violations
test: add integration tests for shipment tracking endpoint
docs: update AGENTS.md with ByJSON response format
```

---

## Out of Scope

- CQRS / MediatR
- Rich Domain Model / Domain Events / Value Objects
- Frontend / mobile client
- Real-time tracking / IoT integration
- Billing or payment processing

---

## Entry Points

| File                                           | Purpose                             |
| ---------------------------------------------- | ----------------------------------- |
| `src/API/Program.cs`                           | Application bootstrap and DI wiring |
| `src/Infrastructure/Data/AppDbContext.cs`      | EF Core DbContext                   |
| `src/Infrastructure/Data/AppDbSeeder.cs`       | Seed data (users, ports, vessels)   |
| `src/Application/Shipments/ShipmentService.cs` | Core shipment business logic        |
| `docs/prds/prd-main.md`                        | Full product requirements           |
