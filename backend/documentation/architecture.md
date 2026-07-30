# Backend Architecture

## Overview

StudentInsights follows **Clean Architecture**, where dependencies always point inward. Business rules remain independent from infrastructure, frameworks, and presentation concerns.

```
StudentInsights.WebApi
        │
        ▼
StudentInsights.Infrastructure
        │
        ▼
StudentInsights.Application
        │
        ▼
StudentInsights.Domain
```

Each layer has a single responsibility and communicates only through well-defined abstractions.

---

# Architecture Layers

## Domain

The Domain layer contains the core business model of the application.

It includes:

- Entities
- Value Objects
- Domain Exceptions
- Enums
- Domain Logic

The Domain project has **no dependency on any other project**.

Entities expose behavior instead of setters. Object creation is performed through static factory methods (for example `Course.Create(...)`), ensuring that business rules are enforced from the moment an entity is created.

Audit information (`CreatedAtUtc`, `UpdatedAtUtc`) together with lifecycle methods (`Delete()`, `Restore()`, `MarkModified()`) are encapsulated inside the entities instead of being manipulated directly by consumers.

---

## Application

The Application layer contains all application use cases.

It is built around **CQRS + MediatR**.

Each feature follows the same structure:

- Commands
- Queries
- Handlers
- DTOs
- Validators
- Mapping Extensions

Application depends only on the Domain layer.

Instead of depending on Infrastructure implementations, it defines interfaces such as:

- `IApplicationDbContext`
- `ICurrentUserService`
- `IJwtTokenGenerator`
- `IPasswordHasher`
- `IEmailSender`

which are implemented by Infrastructure.

Validation is centralized through a MediatR pipeline (`ValidationBehavior<TRequest,TResponse>`), allowing every request with a registered validator to be validated automatically before reaching its handler.

---

## Infrastructure

Infrastructure contains all external implementations.

Examples include:

- Entity Framework Core
- SQL Server
- JWT Authentication
- Password Hashing
- SMTP Email
- Hangfire Background Jobs

Infrastructure implements every interface defined by the Application layer while keeping business rules completely isolated.

The notification system is implemented as a recurring Hangfire job that generates notifications based on upcoming exams, deadlines, goals, and study activity.

---

## WebApi

WebApi is the composition root of the application.

Its responsibilities include:

- Controllers
- Dependency Injection
- Middleware
- Authentication
- Authorization
- Swagger
- Request Pipeline

Controllers contain no business logic.

Every endpoint simply translates an HTTP request into a MediatR Command or Query and converts the returned result into an HTTP response.

---

# Feature Modules

The backend is organized into independent feature modules.

Current modules include:

- Authentication
- Courses
- Learning Activities
- Exams
- Goals
- Study Logs
- Personal Events
- Calendar
- Dashboard
- Analytics
- Notifications
- Admin

The Calendar, Dashboard and Analytics modules are read-only aggregation modules that combine information from multiple entities rather than owning database tables themselves.

---

# Data Access

## Soft Delete

Soft delete is implemented centrally through EF Core Global Query Filters.

Deleted entities are automatically excluded from queries without requiring repeated filtering logic.

Entities that intentionally use hard delete (such as `ClassSchedule` and `SystemSetting`) are excluded from this behavior.

---

## Audit Fields

Audit timestamps are maintained automatically.

Every entity inherits common audit properties, keeping entity implementations clean and consistent.

---

## Optimistic Concurrency

Optimistic concurrency is implemented using `RowVersion`.

Concurrent updates are detected automatically and translated into appropriate API responses.

---

## User Data Isolation

Every handler explicitly filters data by the currently authenticated user.

`ICurrentUserService` resolves the current user's identifier from JWT claims and throws when authentication information is unavailable, preventing accidental data leakage.

---

## Pagination

Pagination is centralized through shared infrastructure.

`PaginationParams` validates and normalizes paging input, while `PaginatedResult<T>` provides a consistent response format for all paginated endpoints.

---

## UTC Date Handling

All API `DateTime` values are normalized to UTC using a custom JSON converter.

This guarantees consistent date handling across the entire application.

---

# Security & Hardening

The backend includes several production-oriented improvements.

## Authentication

- JWT Bearer Authentication
- Refresh Tokens
- Refresh Token Rotation
- Secure Password Hashing
- Email Confirmation
- Password Reset

---

## Request Protection

Authentication endpoints are protected by IP-based Rate Limiting.

Security response headers are applied globally, and HSTS is enabled outside the Development environment.

---

## Logging & Tracing

Structured logging is provided by Serilog.

Every request receives a Correlation ID, making it possible to trace an entire request across middleware, handlers and background jobs.

---

## Error Handling

Global exception handling returns RFC 7807 `ProblemDetails` responses.

The middleware maps common exceptions into meaningful HTTP status codes, including:

- Validation Errors → 400
- Domain Errors → 400
- Unauthorized → 401
- Forbidden → 403
- Not Found → 404
- Concurrency Conflicts → 409
- Unexpected Errors → 500

---

## Background Processing

Hangfire is used for recurring background jobs.

The Hangfire Dashboard is protected and intended for administrative use.

---

# Design Principles

The project follows these principles throughout the codebase:

- Clean Architecture
- SOLID
- CQRS
- Separation of Concerns
- Dependency Injection
- Production-oriented defaults
- Readability over cleverness
- Consistency across feature modules
- Minimal duplication
- No unnecessary abstractions
- No overengineering

Every new feature follows the same architectural conventions, making the project easy to maintain and extend as it grows.