---
title: Backend Techniques (.NET) - ELearning
scope: Clean Architecture · CQRS/MediatR · EF Core · AuthZ · Observability
status: active
---

# Backend Techniques (.NET) - ELearning

This guide summarizes the backend engineering techniques currently used in this repo and where to find them.

## Clean Architecture + Dependency Injection

- The solution is split into `Domain`, `Core`, `Application`, `Infrastructure`, and `WebApi`.
- Service registration is centralized via:
  - `ELearning.Application/DependencyInjection.cs` (MediatR, validators, pipeline behaviors)
  - `ELearning.Infrastructure/DependencyInjection.cs` (EF Core, repositories, auth services, storage provider)

## CQRS with MediatR (Commands + Queries)

- API controllers are thin: they translate HTTP requests into MediatR commands/queries.
- Handlers live under `src/ELearning.Application/Features/...`.
- Responses use `ELearning.Core.Common.Result`:
  - Success returns typed DTOs (or `Result` for 204/no body)
  - Failures return `Error` objects that the WebApi converts to `application/problem+json`

## Validation / Logging / Performance pipelines (MediatR behaviors)

The repo uses MediatR pipeline behaviors to keep handlers clean:

1. `ValidationBehavior<TRequest, TResponse>`
   - Runs all `FluentValidation` validators for the request
   - Throws `ELearning.Core.Exceptions.ValidationException` on failures
2. `LoggingBehavior<TRequest, TResponse>`
   - Logs request start/end via `ILogger`
3. `PerformanceBehavior<TRequest, TResponse>`
   - Warns when a request exceeds a slow threshold (500ms)

These are wired in `src/ELearning.Application/DependencyInjection.cs`.

## EF Core patterns (model configuration, soft delete, audit)

### DbContext configuration

- `src/ELearning.Infrastructure/Persistence/ApplicationDbContext.cs`
  - Applies entity configurations from the assembly via:
    - `builder.ApplyConfigurationsFromAssembly(...)`
  - Implements audit fields in `SaveChangesAsync(...)`:
    - `CreatedAt/CreatedBy`, `UpdatedAt/UpdatedBy`
    - soft delete: convert `EntityState.Deleted` into `IsDeleted = true`

### Entity configuration

- Each entity has an EF configuration under:
  - `src/ELearning.Infrastructure/Persistence/Configurations/*.cs`
- Course entities follow the same approach:
  - explicit table/column mapping
  - query filters for soft delete: `builder.HasQueryFilter(x => !x.IsDeleted)`

### Migrations

- Migrations are generated and applied by EF Core.
- This repo uses a local `dotnet-ef` tool (`dotnet-tools.json`) to avoid global tooling/version mismatches.

## Repository + Unit of Work

- Repository abstraction:
  - `src/ELearning.Core/Abstractions/IRepository.cs`
  - Feature-specific repositories extend this (e.g. `ICourseRepository`)
- The `DbContext` implements `IUnitOfWork`:
  - `ApplicationDbContext` exposes `SaveChangesAsync(...)`

Usage pattern in handlers:
- Load entity from repository
- Mutate aggregate (domain methods)
- Call `repository.Update(...)` when needed
- `await unitOfWork.SaveChangesAsync(ct)`

## Permission-based authorization (policy + handler)

- Permissions are declared in `ELearning.Core.Constants.Permissions`.
- Authorization uses:
  - `[HasPermission(Permissions.X.Y)]` on controller actions
  - a custom `PermissionPolicyProvider`
  - `PermissionAuthorizationHandler` that maps a user’s roles to permissions via `PermissionMap`

Find:
- Policy plumbing: `src/ELearning.WebApi/Authorization/*`
- Permission mapping: `src/ELearning.Core/Constants/PermissionMap.cs`

## Error handling (problem+json)

- `src/ELearning.WebApi/Middlewares/ExceptionHandlingMiddleware.cs`
  - Converts known exceptions:
    - `ValidationException` → `422 Unprocessable Entity`
    - `NotFoundException` → `404`
    - other auth-related exceptions → `401/403`
  - Returns `Content-Type: application/problem+json`

Handlers mostly return `Result<T>` and controllers call `Problem(result.Error)`.

## Observability: Serilog + CorrelationId

- Serilog structured logging is configured in `src/ELearning.WebApi/Program.cs`.
- Every request propagates `X-Correlation-Id`:
  - `src/ELearning.WebApi/Middlewares/CorrelationIdMiddleware.cs`

## File upload abstraction (storage provider swap)

- Abstraction:
  - `src/ELearning.Core/Abstractions/IFileStorage.cs`
  - `UploadLessonAssetCommandHandler` depends only on `IFileStorage.SaveAsync(...)`
- Local implementation:
  - `src/ELearning.Infrastructure/Storage/LocalFileStorage.cs`
- File download in current MVP:
  - `src/ELearning.WebApi/Controllers/v1/AssetsController.cs`

Provider swapping is described here:
- `docs/file-upload-storage-guide.md`

## Sprint 3: Scheduled classes (cohorts) + sessions

- **Domain**: `ELearning.Domain/Aggregates/TrainingClassAggregate/` — `TrainingClass` (root), `ClassSession`, `ClassInstructor`; enums `ClassSessionType` (Zoom / Offline / Vod), statuses.
- **API**: `ELearning.WebApi/Controllers/v1/TrainingClassesController.cs` — route prefix `api/v1/training-classes`.
- **Permissions**: `Permissions.Classes.*` (`Read`, `Create`, `Update`, `ManageSessions`) in `PermissionMap` for Admin, OrgAdmin, Instructor; Learner has `Classes.Read`.
- **Zoom (abstraction)**: `IZoomMeetingService` + `ZoomMeetingInfo` in Core; default implementation `NoOpZoomMeetingService` in Infrastructure (placeholder join URL). Swap for real Zoom REST calls in production.
- **Conflict rules**: `ITrainingClassRepository.HasInstructorSessionOverlapAsync` — overlap against non-cancelled sessions for any class where the user is an instructor.
- **EF**: tables `training_classes`, `class_sessions`, `class_instructors`; FK from `training_classes.course_id` → `courses.id`; migration `20260405121500_Sprint3_TrainingClassesAndSessions.cs`.

## Database construct: SQL Server View

In this stack, a SQL Server `VIEW` is a **database-level construct** (not a .NET architecture pattern).
It defines a virtual “table” backed by a `SELECT` query.

Typical reasons to use views:

- Reuse query logic across multiple APIs/UI screens
- Provide a stable shape for reporting/read models
- Encapsulate joins/filters and expose only the needed columns
- Centralize soft-delete predicates (e.g., show only active rows)

Notes for backend (.NET) usage:

- EF Core can query views like normal tables (read-only scenarios are common).
- If you need write/update through the view, you must ensure SQL Server supports it for your specific view (often not recommended).
- For performance-critical paths, consider whether a view should be replaced by a dedicated query/projection or an indexed view (SQL Server feature).

