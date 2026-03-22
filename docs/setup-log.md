---
title: Setup Log – ELearning Platform
scope: Backend (.NET 10 Clean Architecture) + Frontend (Angular 21) + Infrastructure (Docker)
status: completed
created: 2026-03-22
---

# Setup Log – ELearning Platform

This document records every tool, package, version, and decision made during the initial project setup (`src/` init).

---

## 1. Environment & Toolchain

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 10.0.100 | Backend runtime and build toolchain |
| Node.js | 22.20.0 | Frontend runtime |
| npm | 10.9.3 | Frontend package manager |
| Angular CLI | 21.0.0 | Angular workspace scaffold + build |
| Docker (optional) | latest | Containerised local dev environment |

> **Note on .NET version**: Projects were initially created targeting `net9.0` but upgraded to `net10.0` because `Npgsql.EntityFrameworkCore.PostgreSQL 10.x` requires `.NETCoreApp 10.0`. Since the installed SDK is 10.0.100, `net10.0` is the correct target.

---

## 2. Project Layout

```
ELearning/
├── src/                         ← .NET backend root
│   ├── ELearning.sln
│   ├── ELearning.Domain/
│   ├── ELearning.Core/
│   ├── ELearning.Application/
│   ├── ELearning.Infrastructure/
│   └── ELearning.WebApi/
├── tests/
│   ├── ELearning.Domain.UnitTests/
│   ├── ELearning.Application.UnitTests/
│   └── ELearning.Architecture.Tests/
├── frontend/                    ← Angular workspace
│   ├── src/app/
│   │   ├── core/
│   │   ├── shared/
│   │   └── features/
│   ├── Dockerfile
│   └── nginx.conf
├── docs/
├── scripts/
├── docker-compose.yml
└── README.md
```

---

## 3. Backend – .NET Solution

### 3.1 Solution & Projects Created

```bash
dotnet new sln -n ELearning                                          # solution file
dotnet new classlib -n ELearning.Domain       --framework net10.0
dotnet new classlib -n ELearning.Core         --framework net10.0
dotnet new classlib -n ELearning.Application  --framework net10.0
dotnet new classlib -n ELearning.Infrastructure --framework net10.0
dotnet new webapi   -n ELearning.WebApi       --framework net10.0 --no-openapi
```

### 3.2 Project References (Clean Architecture dependency rules)

```
Domain          ← (no dependencies)
Core            ← Domain
Application     ← Domain, Core
Infrastructure  ← Domain, Core, Application
WebApi          ← Domain, Core, Application, Infrastructure
```

```bash
dotnet add ELearning.Core          reference ELearning.Domain
dotnet add ELearning.Application   reference ELearning.Domain ELearning.Core
dotnet add ELearning.Infrastructure reference ELearning.Domain ELearning.Core ELearning.Application
dotnet add ELearning.WebApi        reference ELearning.Domain ELearning.Core ELearning.Application ELearning.Infrastructure
```

---

## 4. NuGet Packages

### 4.1 ELearning.Domain

| Package | Version | Reason |
|---|---|---|
| `MediatR.Contracts` | 2.0.1 | `INotification` interface for domain events (lightweight – no MediatR runtime in Domain) |

### 4.2 ELearning.Application

| Package | Version | Reason |
|---|---|---|
| `MediatR` | 14.1.0 | CQRS – commands, queries, pipeline behaviors |
| `FluentValidation` | 12.1.1 | Request validation in pipeline |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | `AddValidatorsFromAssembly` DI scanner |
| `AutoMapper` | 16.1.1 | Object-to-object mapping (profile-based) |

> **AutoMapper note**: v16.x bundles DI support directly — `AutoMapper.Extensions.Microsoft.DependencyInjection` is no longer needed. Use `services.AddAutoMapper(c => c.AddMaps(assembly))`.

### 4.3 ELearning.Infrastructure

| Package | Version | Reason |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 | EF Core provider for PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` | (latest) | EF Core migrations tooling |
| `Serilog.AspNetCore` | (latest) | Structured logging integration |
| `StackExchange.Redis` | (latest) | Redis cache client |
| `Hangfire.Core` | 1.8.23 | Background job scheduling |
| `Hangfire.PostgreSql` | 1.21.1 | Hangfire PostgreSQL storage backend |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | JWT Bearer authentication middleware |
| `Newtonsoft.Json` | 13.0.4 | Pinned to override Hangfire's transitive dep on vulnerable 11.0.1 |

> **Security note**: `Hangfire.Core` 1.8.x pulls in `Newtonsoft.Json 11.0.1` which has a known high-severity vulnerability ([GHSA-5crp-9r3c-p9vr](https://github.com/advisories/GHSA-5crp-9r3c-p9vr)). A direct reference to `Newtonsoft.Json 13.0.4` was added to override the vulnerable transitive version.

### 4.4 ELearning.WebApi

| Package | Version | Reason |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.5 | Built-in OpenAPI document generation (`AddOpenApi`, `MapOpenApi`) |
| `Scalar.AspNetCore` | 2.13.13 | Modern API documentation UI (replaces Swagger UI) — served at `/scalar` |
| `Asp.Versioning.Mvc` | 8.1.1 | API versioning (`v1`, `v2` routes) |

---

## 5. Test Projects

### 5.1 Projects Created

```bash
dotnet new xunit -n ELearning.Domain.UnitTests      --framework net10.0
dotnet new xunit -n ELearning.Application.UnitTests --framework net10.0
dotnet new xunit -n ELearning.Architecture.Tests    --framework net10.0
```

### 5.2 Test Project Packages

| Project | Package | Version | Purpose |
|---|---|---|---|
| Domain.UnitTests | `FluentAssertions` | 8.9.0 | Readable assertion syntax |
| Application.UnitTests | `FluentAssertions` | 8.9.0 | Readable assertion syntax |
| Application.UnitTests | `NSubstitute` | 5.3.0 | Mocking/substitution for interfaces |
| Architecture.Tests | `NetArchTest.Rules` | 1.3.2 | Enforce Clean Architecture layer dependency rules |

### 5.3 Test Project References

| Test Project | References |
|---|---|
| Domain.UnitTests | `ELearning.Domain` |
| Application.UnitTests | `ELearning.Application` |
| Architecture.Tests | `ELearning.Domain`, `ELearning.Application`, `ELearning.Infrastructure`, `ELearning.WebApi` |

---

## 6. Base Files Scaffolded

### 6.1 Domain Layer

| File | Purpose |
|---|---|
| `Domain/Shared/Entity.cs` | Base entity with `Guid Id`, equality by identity |
| `Domain/Shared/AggregateRoot.cs` | Extends Entity; manages `IDomainEvent` list |
| `Domain/Shared/IDomainEvent.cs` | Marker interface extends `INotification` |
| `Domain/Shared/DomainEvent.cs` | Abstract record with `EventId` + `OccurredAt` |
| `Domain/Shared/ValueObject.cs` | Structural equality via `GetEqualityComponents()` |
| `Domain/Exceptions/DomainException.cs` | Business rule violation exception |

### 6.2 Core Layer (Shared Kernel)

| File | Purpose |
|---|---|
| `Core/Common/Result.cs` | `Result` / `Result<T>` – explicit success/failure without exceptions |
| `Core/Common/Error.cs` | `Error` record with factory methods (`NotFound`, `Conflict`, `Validation`, `Unauthorized`) |
| `Core/Common/PagedList.cs` | Typed paginated list with metadata |
| `Core/Abstractions/IRepository.cs` | Generic repository contract (`GetById`, `Find`, `Add`, etc.) |
| `Core/Abstractions/IUnitOfWork.cs` | `SaveChangesAsync` contract |
| `Core/Abstractions/ICurrentUserService.cs` | Current authenticated user accessor |
| `Core/Exceptions/NotFoundException.cs` | Typed 404 exception |
| `Core/Exceptions/ValidationException.cs` | Validation failure with error dictionary |

### 6.3 Application Layer

| File | Purpose |
|---|---|
| `Application/Common/Behaviors/ValidationBehavior.cs` | MediatR pipeline – runs FluentValidation, throws `ValidationException` |
| `Application/Common/Behaviors/LoggingBehavior.cs` | MediatR pipeline – logs request name before/after |
| `Application/Common/Behaviors/PerformanceBehavior.cs` | MediatR pipeline – warns if handler > 500ms |
| `Application/Common/Interfaces/IEmailService.cs` | Email abstraction (plain + template) |
| `Application/Common/Interfaces/IPaymentService.cs` | Payment gateway abstraction (create, verify, refund) |
| `Application/Common/Interfaces/IZoomService.cs` | Zoom API abstraction (create meeting, get participants) |
| `Application/DependencyInjection.cs` | Registers MediatR + pipeline behaviors + FluentValidation + AutoMapper |

### 6.4 Infrastructure Layer

| File | Purpose |
|---|---|
| `Infrastructure/Persistence/ApplicationDbContext.cs` | EF Core DbContext; implements `IUnitOfWork`; applies audit fields and soft-delete on `SaveChangesAsync` |
| `Infrastructure/Persistence/Repositories/GenericRepository.cs` | Generic EF Core repository implementation |
| `Infrastructure/Identity/CurrentUserService.cs` | Reads `UserId`, `Email`, `Roles` from `HttpContext.User` claims |
| `Infrastructure/DependencyInjection.cs` | Registers DbContext (Npgsql), `IUnitOfWork`, `ICurrentUserService` |

### 6.5 WebApi Layer

| File | Purpose |
|---|---|
| `WebApi/Program.cs` | Bootstraps app: Application + Infrastructure DI, JWT Bearer auth, CORS, API versioning, OpenAPI + Scalar UI |
| `WebApi/appsettings.json` | Production config template (empty secrets) |
| `WebApi/appsettings.Development.json` | Local dev config (PostgreSQL, Redis, JWT, CORS) |
| `WebApi/Dockerfile` | Multi-stage Docker build for API image |

---

## 7. Frontend – Angular

### 7.1 Workspace Created

```bash
ng new elearning-web \
  --directory frontend \
  --routing true \
  --style scss \
  --skip-git true \
  --no-interactive
```

### 7.2 Angular CLI Options Explained

| Flag | Value | Reason |
|---|---|---|
| `--routing` | `true` | Generates `app.routes.ts` for lazy-loaded feature routes |
| `--style` | `scss` | SCSS for maintainable, component-scoped styles |
| `--skip-git` | `true` | Monorepo already has root git |
| `--no-interactive` | — | Non-interactive for scripted setup |

### 7.3 Folder Structure Created

```
frontend/src/app/
├── core/
│   ├── interceptors/    # HTTP interceptors: auth token, error, loading
│   ├── guards/          # Route guards: auth, role
│   ├── services/        # Singleton services: auth, api client
│   └── models/          # Shared TypeScript interfaces/types
├── shared/
│   ├── components/      # Reusable UI components (button, modal, table, etc.)
│   ├── pipes/           # Custom Angular pipes
│   └── directives/      # Custom Angular directives
└── features/
    ├── auth/            # Login, register, password reset
    ├── courses/         # Course catalog, detail, builder
    ├── learning/        # Lesson viewer, progress, quiz
    └── dashboard/       # Learner & admin dashboards
```

### 7.4 Angular Version Info

| Package | Version |
|---|---|
| Angular CLI | 21.0.0 |
| Node.js | 22.20.0 |

---

## 8. Infrastructure – Docker

### 8.1 Files Created

| File | Purpose |
|---|---|
| `docker-compose.yml` | Orchestrates API + PostgreSQL + Redis + Frontend containers |
| `src/ELearning.WebApi/Dockerfile` | Multi-stage .NET API Docker image |
| `frontend/Dockerfile` | Multi-stage Angular build → nginx serve |
| `frontend/nginx.conf` | Nginx config: Angular SPA fallback + `/api` proxy to backend |

### 8.2 Services in docker-compose

| Service | Image | Port | Notes |
|---|---|---|---|
| `api` | Built from `src/` | 5000:8080 | Depends on `postgres` (healthy) + `redis` (healthy) |
| `postgres` | `postgres:17-alpine` | 5432:5432 | Health-checked; data persisted in named volume |
| `redis` | `redis:7-alpine` | 6379:6379 | Health-checked; data persisted in named volume |
| `frontend` | Built from `frontend/` | 4200:80 | Served via nginx; proxies `/api` to backend |

### 8.3 Quick Start

```bash
# Start all services
docker compose up -d

# Run migrations (first time)
dotnet ef database update --project src/ELearning.Infrastructure --startup-project src/ELearning.WebApi

# Stop services
docker compose down

# Reset data volumes
docker compose down -v
```

---

## 9. Build Verification

```bash
# Full solution build
dotnet build src/ELearning.sln --no-restore

# Run all tests
dotnet test src/ELearning.sln

# Angular local dev
cd frontend && npm start
```

**Build result**: ✅ `Build succeeded. 0 Error(s)` (all 8 projects)

---

## 10. Issues Encountered & Resolutions

| # | Issue | Root Cause | Resolution |
|---|---|---|---|
| 1 | `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1` incompatible with `net9.0` | Npgsql 10.x targets `net10.0` only | Upgraded all projects from `net9.0` → `net10.0` |
| 2 | `AddValidatorsFromAssembly` not found | `FluentValidation.DependencyInjectionExtensions` was missing | Added `FluentValidation.DependencyInjectionExtensions 12.1.1` |
| 3 | `AddAutoMapper(assembly)` error | `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1` conflicts with `AutoMapper 16.x` | Removed extension package; used `services.AddAutoMapper(c => c.AddMaps(assembly))` (v16 built-in) |
| 4 | `AddOpenApi` / `MapOpenApi` not found | WebApi created with `--no-openapi` flag; missing OpenApi package | Added `Microsoft.AspNetCore.OpenApi 10.0.5` |
| 5 | `Newtonsoft.Json 11.0.1` vulnerability warning | Hangfire transitive dependency on old Newtonsoft.Json | Pinned `Newtonsoft.Json 13.0.4` directly in Infrastructure project |
| 6 | Shell tool returning empty output | Shell session stuck in stateful context | Used explicit `working_directory` parameter on all subsequent Shell calls |

---

## Related Documents

| Document | Description |
|---|---|
| [`project-structure.md`](./project-structure.md) | Full folder layout reference |
| [`phases.md`](./phases.md) | Feature delivery phases roadmap |
| [`sprint-plan.md`](./sprint-plan.md) | Sprint-by-sprint task breakdown |
| [`erd.md`](./erd.md) | Entity relationship diagram |
| [`business-analysis.md`](./business-analysis.md) | Business requirements per module |
| [`advanced-architecture-notes.md`](./advanced-architecture-notes.md) | Pricing, concurrency, Zoom integration patterns |
