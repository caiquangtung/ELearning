---
title: API design guidelines
scope: REST · Versioning · Errors · Auth
status: active
---

## Versioning

- All endpoints are versioned using the route prefix: `api/v{version}`.
- Current baseline: **v1** (see controllers under `src/ELearning.WebApi/Controllers/v1/`).

## Authentication

- Auth is **JWT Bearer**.
- Prefer permission-based authorization for protected operations.

## Authorization

- Use `[HasPermission(Permissions.X.Y)]` on controller actions where applicable.
- Policies are created dynamically by `PermissionPolicyProvider` using `Permission:{permission}`.

## Responses

- Success responses should return typed DTOs.
- Errors use `application/problem+json` (see `ExceptionHandlingMiddleware` and controller `Problem(Error)` helpers).

## Naming

- Use nouns for resources: `/organizations`, `/users`.
- Use sub-resources for relationships: `/organizations/{id}/members`.

