---
title: Sprint 5 completion — License Pools (B2B)
status: done
---

## Goal

Implement B2B **license pooling** and basic **seat assignment** for organizations.

## Delivered (MVP)

### Backend

- **Domain**: `LicensePool` + `LicenseAssignment` aggregate (`src/ELearning.Domain/Aggregates/LicensePoolAggregate/*`)
  - Enforces **seat availability** and prevents duplicate active assignments for the same user.
  - Prevents assignment if pool is **expired**.
- **Persistence**:
  - `license_pools`, `license_assignments` EF mappings (`src/ELearning.Infrastructure/Persistence/Configurations/*`)
  - Repository: `ILicensePoolRepository` + `LicensePoolRepository`
  - Migration: `20260430162719_Sprint5_LicensePools` (`src/ELearning.Infrastructure/Persistence/Migrations/`)
- **API** (`api/v1`):
  - `GET /organizations/{organizationId}/license-pools`
  - `POST /organizations/{organizationId}/license-pools`
  - `GET /license-pools/{id}`
  - `GET /license-pools/{id}/usage`
  - `POST /license-pools/{id}/assignments`
  - `DELETE /license-pools/{id}/assignments/{userId}`

### Frontend (Angular)

- **Routes**
  - `GET /organizations/:id/license-pools` (list + create)
  - `GET /license-pools/:id` (detail + assign/revoke)
- **Screens**
  - `features/licenses/license-pool-list.component.ts`
  - `features/licenses/license-pool-detail.component.ts`
- **Org entrypoint**: Organization detail now links to “License pools”.

## Deferred / follow-ups

- Enrollment flows: “assign license then enroll member into a class/course”.
- Bulk assignment and expiry warnings in UI.

## Notes

- EFCore package alignment was required to remove MSBuild conflicts (MSB3277) and keep the build warning-free.

## Validation

- `dotnet test src/ELearning.sln`

