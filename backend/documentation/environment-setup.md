# StudentInsights — Backend API

StudentInsights is a personal academic growth platform that helps students organize their academic life in one place. It provides tools for managing courses, learning activities, exams, goals, study logs, personal events, notifications, and academic analytics through a secure and well-structured RESTful API.

This repository contains the backend of the application, built with **ASP.NET Core 8**, **Entity Framework Core**, and **Clean Architecture**, with a strong focus on maintainability, scalability, and production-ready development practices.

---

# Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core 8** (SQL Server)
- **Clean Architecture**
- **CQRS + MediatR**
- **FluentValidation**
- **JWT Authentication & Refresh Tokens**
- **Hangfire** (Background Jobs)
- **Serilog** (Structured Logging)
- **Swagger / OpenAPI**
- **SQL Server**

For a detailed explanation of the architecture, see:

- `documentation/architecture.md`
- `documentation/environment-setup.md`

---

# Prerequisites

Before running the project, make sure the following are installed:

- .NET 8 SDK
- SQL Server (LocalDB, SQL Server Express, Developer Edition, or Docker)
- Visual Studio 2022 (17.10+) or any IDE with .NET support

---

# Getting Started

## 1. Clone the repository

```bash
git clone <repository-url>
cd StudentInsights
```

## 2. Restore packages

```bash
dotnet restore StudentInsights.sln
```

## 3. Configure User Secrets

Sensitive configuration values are intentionally excluded from source control.

Configure the required secrets using:

```bash
cd StudentInsights.WebApi

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>"

dotnet user-secrets set "Jwt:Secret" "<32+ character secret>"

dotnet user-secrets set "Email:SmtpUsername" "<smtp-username>"

dotnet user-secrets set "Email:SmtpPassword" "<smtp-password>"
```

Additional configuration options are documented in:

`documentation/environment-setup.md`

---

## 4. Apply Database Migrations

```bash
dotnet ef database update \
--project StudentInsights.Infrastructure \
--startup-project StudentInsights.WebApi
```

---

## 5. Run the Application

```bash
dotnet run --project StudentInsights.WebApi
```

When running in the Development environment, Swagger UI is available at:

```
/swagger
```

---

# Main Features

- JWT Authentication
- Refresh Token Rotation
- Email Confirmation
- Password Reset
- Course Management
- Learning Activity Management
- Exam Management
- Goal Tracking
- Study Log Management
- Personal Calendar
- Dashboard
- Academic Analytics
- Notification System
- Admin Module
- Background Notification Jobs

---

# Background Services

## Hangfire Dashboard

The project uses Hangfire to execute scheduled background jobs such as notification generation.

Dashboard endpoint:

```
/hangfire
```

Outside the Development environment, the dashboard is protected using HTTP Basic Authentication.

---

## Health Check

Health endpoint:

```
/health
```

Used for monitoring database connectivity and application availability.

---

# Testing

Run all tests using:

```bash
dotnet test StudentInsights.sln
```

The automated test suite is actively being expanded. The solution already includes a dedicated test project and CI pipeline support, allowing new tests to be integrated seamlessly as they are added.

---

# Project Structure

```
StudentInsights.Domain/
    Domain entities, value objects, enums and domain logic.

StudentInsights.Application/
    CQRS commands, queries, handlers, DTOs, validators and interfaces.

StudentInsights.Infrastructure/
    Entity Framework Core, authentication, persistence, email services,
    background jobs and external integrations.

StudentInsights.WebApi/
    Controllers, middleware, dependency injection,
    authentication and application startup.

StudentInsights.Application.UnitTests/
    Unit tests.
```

For a complete architectural overview, see:

`documentation/architecture.md`

---

# Production Readiness

The backend has been hardened for production use and includes:

- Clean Architecture
- CQRS with MediatR
- JWT Authentication
- Refresh Token Rotation
- Secure Password Hashing
- Rate Limiting for Authentication Endpoints
- Security Response Headers
- Structured Logging with Serilog
- Correlation ID Tracing
- Global Exception Handling (RFC 7807 ProblemDetails)
- Optimistic Concurrency Handling
- Soft Delete
- Audit Fields
- Per-user Data Isolation
- Centralized Pagination
- UTC Date Handling
- Background Processing with Hangfire
- CI Pipeline
- Comprehensive Project Documentation

The project is designed to be clean, maintainable, extensible, and suitable as a solid foundation for future development.