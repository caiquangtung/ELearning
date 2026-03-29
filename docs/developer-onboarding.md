---
title: Developer onboarding
scope: Local setup · Docker · Backend run/debug
status: active
---

## Prerequisites

- Docker Desktop
- .NET SDK (matches repo `TargetFramework`, currently `net10.0`)

## Run everything (Docker Compose)

From repo root:

```bash
docker compose up --build
```

Services:

- **API**: `http://localhost:5000` (container port 8080)
- **Postgres**: `localhost:5432`
- **Redis**: `localhost:6379`
- **Frontend nginx**: `http://localhost:4200` (static build container)

## Run API locally (without Docker)

1. Start Postgres + Redis (either via Docker Compose or locally).
2. Configure `src/ELearning.WebApi/appsettings.Development.json`:
   - `ConnectionStrings:DefaultConnection`
   - `JwtSettings:*`
   - `Seed:*` (Development only)
3. Run:

```bash
dotnet run --project src/ELearning.WebApi
```

On startup the API runs migrations + seeds the default admin (Development only).

## Tests

```bash
dotnet test src/ELearning.sln
```

## Logging & correlation id

- Logs are structured via **Serilog** (JSON console).
- Each request gets/propagates `X-Correlation-Id` (see `CorrelationIdMiddleware`).

