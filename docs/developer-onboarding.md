---
title: Developer onboarding
scope: Local setup · Docker · Backend · Angular spec
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

## Frontend (Angular)

The .NET backend lives under `src/`; the Angular SPA is under **`frontend/web`** (see `frontend/README.md` and `docs/sprint4-completion.md`). For engineering conventions (spec, state, patterns), see:

- [`docs/spec/angular-frontend-spec.md`](spec/angular-frontend-spec.md) (canonical Angular / PrimeNG spec; [`docs/angular-frontend-spec.md`](angular-frontend-spec.md) redirects here)

Run the UI locally: from `frontend/web`, `npm install` then `npm start` (API on `http://localhost:5000` with CORS for `http://localhost:4200`).

## Run API locally (without Docker)

1. Start Postgres + Redis (either via Docker Compose or locally).
2. Configure `src/ELearning.WebApi/appsettings.Development.json`:
   - `ConnectionStrings:DefaultConnection`
   - `JwtSettings:*`
   - `Seed:*` (Development only)
   - `Storage:Local:BasePath` (optional — where uploaded assets are stored)
3. Run:

```bash
dotnet run --project src/ELearning.WebApi
```

On startup the API runs migrations + seeds the default admin (Development only).

## EF Core migrations (local tool)

This repo uses a **local** `dotnet-ef` tool (pinned in `dotnet-tools.json`).

From repo root:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations list --project src/ELearning.Infrastructure --startup-project src/ELearning.WebApi
```

## Local content storage (Sprint 2)

- Uploaded lesson assets are stored on disk when using the local storage provider.
- Default is a `storage/` folder under the API output directory, unless overridden.

See also: `docs/file-upload-storage-guide.md` for how to swap to S3/Azure Blob.

Example config:

```json
"Storage": {
  "Local": {
    "BasePath": "/absolute/path/to/ELearning-storage"
  }
}
```

## Tests

```bash
dotnet test src/ELearning.sln
```

## Logging & correlation id

- Logs are structured via **Serilog** (JSON console).
- Each request gets/propagates `X-Correlation-Id` (see `CorrelationIdMiddleware`).

