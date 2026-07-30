# StudentInsights — Backend API

StudentInsights is a personal academic growth platform: a single place for a
student to manage courses, assignments, exams, goals, and their personal
calendar, and to see their own progress reflected back through a dashboard
and analytics rather than piecing it together across five different apps.
This repository contains the backend API, built with ASP.NET Core 8 using
Clean Architecture, CQRS, Entity Framework Core, and SQL Server, with a
strong focus on maintainability, security, and production readiness.

## Tech stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core 8** (SQL Server)
- **Clean Architecture** (Domain → Application → Infrastructure → WebApi)
- **CQRS + MediatR**, with FluentValidation running as a pipeline behavior
- **JWT Bearer authentication**, with refresh tokens and email confirmation/password reset via SMTP
- **Hangfire** for the recurring notification-generation job
- **Serilog** (console + rolling file sinks), correlation-ID tracing
- **Swagger / OpenAPI** via Swashbuckle

See [`documentation/architecture.md`](documentation/architecture.md) for how
the layers and feature modules fit together, and
[`documentation/environment-setup.md`](documentation/environment-setup.md)
for every configuration value a new environment needs.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, a local SQL Server instance, or a container)
- Visual Studio 2022 (17.10+) or any editor with C# support

## Getting started

1. **Clone and restore**

   ```bash
   git clone <repository-url>
   cd StudentInsights
   dotnet restore StudentInsights.sln
   ```

2. **Configure secrets.** `appsettings.json` intentionally ships with the
   `Jwt:Secret` and `Email:Smtp*` credentials blank — the app fails fast at
   startup if they're missing, rather than failing confusingly on first
   request. Set them with `dotnet user-secrets` (never commit real values):

   ```bash
   cd StudentInsights.WebApi
   dotnet user-secrets set "Jwt:Secret" "<a random string, 32+ characters>"
   dotnet user-secrets set "Email:SmtpUsername" "<smtp username>"
   dotnet user-secrets set "Email:SmtpPassword" "<smtp password>"
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
   ```

   Full list of required/optional values:
   [`documentation/environment-setup.md`](documentation/environment-setup.md).

3. **Apply migrations**

   ```bash
   dotnet ef database update --project StudentInsights.Infrastructure --startup-project StudentInsights.WebApi
   ```

4. **Run the API**

   ```bash
   dotnet run --project StudentInsights.WebApi
   ```

   By default this serves on the ports configured in
   `StudentInsights.WebApi/Properties/launchSettings.json`. In Development,
   Swagger UI is available at `/swagger`.

## API documentation

In Development, Swagger UI is available at `/swagger`, where every endpoint
is documented and can be tested directly using JWT Bearer authentication.

## Background jobs & operational endpoints

- **`/hangfire`** — the Hangfire dashboard for the recurring notification
  job. Open in Development; gated behind HTTP Basic Auth
  (`Hangfire:DashboardUsername` / `Hangfire:DashboardPassword`) everywhere
  else.
- **`/health`** — anonymous liveness/readiness probe (checks database
  connectivity), for uptime monitors and container orchestrators.

## Testing

```bash
dotnet test StudentInsights.sln
```

`StudentInsights.Application.UnitTests` is the current test project
(xUnit). It's still early — most of the suite is yet to be written — but
`dotnet test`/the CI pipeline (`.github/workflows/backend-ci.yml`) already
run against it, so new tests are picked up automatically as they're added.

## Project structure

```
StudentInsights.Domain/           Entities, value objects, domain exceptions — no outward dependencies
StudentInsights.Application/      CQRS commands/queries, validators, DTOs, interfaces (depends only on Domain)
StudentInsights.Infrastructure/   EF Core, JWT/password hashing, SMTP email, background jobs
StudentInsights.WebApi/           Controllers, middleware, composition root (Program.cs)
StudentInsights.Application.UnitTests/
```

See [`documentation/architecture.md`](documentation/architecture.md) for the
full breakdown, including the ten feature modules under
`StudentInsights.Application/Features`.

## Production readiness

This API has been through a dedicated hardening pass: rate limiting on the
authentication endpoints, standard security response headers, structured
logging with per-request correlation IDs, optimistic-concurrency conflicts
surfaced as `409 Conflict` instead of a generic `500`. Soft delete, audit
fields, per-user data isolation, and centralized pagination were already in
place from the original build. See
[`documentation/architecture.md`](documentation/architecture.md) for details.