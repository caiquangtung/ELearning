---
title: Sprint 3 completion notes — Class scheduling & session management
status: Backend MVP delivered (Zoom prod + UI + integration tests deferred)
---

## Delivered (backend MVP)

### Domain

- **Aggregate**: `ELearning.Domain/Aggregates/TrainingClassAggregate/`
  - `TrainingClass` (root): `CourseId`, `Title`, `MaxLearners`, `Status`, soft delete, `Instructors`, `Sessions`
  - `ClassInstructor`: `TrainingClassId`, `UserId`, `AssignedAt`
  - `ClassSession`: `SessionType` (Zoom/Offline/Vod), `StartUtc`, `EndUtc`, optional `Location`, Zoom fields, `Status` + cancel/update

### Statuses (documented)

- **`TrainingClassStatus`**
  - `Draft`: mới tạo, chưa có session
  - `Scheduled`: đã có ít nhất 1 session (khi gọi `ScheduleSession`)
  - `InProgress`: reserved cho sprint sau (khi có enrollment/attendance bắt đầu)
  - `Completed`: reserved cho sprint sau (khi lớp kết thúc)
  - `Cancelled`: lớp bị hủy
- **`ClassSessionStatus`**
  - `Scheduled`: session đang hiệu lực
  - `Cancelled`: session bị hủy (không tính vào overlap check)

### Core

- **Permissions**: `Permissions.Classes.*` (`Read`, `Create`, `Update`, `ManageSessions`)
- **AuthZ map**: `PermissionMap` updated for OrgAdmin/Instructor/Learner
- **Repositories / services**:
  - `ITrainingClassRepository`
  - `IZoomMeetingService` + `ZoomMeetingCreateRequest` + `ZoomMeetingInfo`

### Application (MediatR)

| Feature | Handler |
|--------|---------|
| Create training class | `CreateTrainingClassCommandHandler` |
| Update training class | `UpdateTrainingClassCommandHandler` |
| Delete training class | `DeleteTrainingClassCommandHandler` |
| Assign/remove instructor | `AssignInstructorCommandHandler`, `RemoveInstructorCommandHandler` |
| Schedule/update/cancel session | `ScheduleSessionCommandHandler`, `UpdateSessionCommandHandler`, `CancelSessionCommandHandler` |
| Get training class detail | `GetTrainingClassQueryHandler` |
| List training classes | `ListTrainingClassesQueryHandler` |

Business rules implemented:
- **Only published courses** can be used to create a training class.
- **Offline session** requires `Location`.
- **Instructor conflict detection**: prevent scheduling a session if any assigned instructor has another overlapping (non-cancelled) session.

### Infrastructure

- **EF Core**:
  - Configurations: `TrainingClassConfiguration`, `ClassInstructorConfiguration`, `ClassSessionConfiguration`
  - `ApplicationDbContext`: adds `DbSet<TrainingClass> TrainingClasses`
  - Repository: `TrainingClassRepository` (includes overlap check)
- **Zoom integration placeholder**:
  - `NoOpZoomMeetingService` registered for `IZoomMeetingService` (dev-friendly stub; generates placeholder join URL)

### Web API

Controller: `ELearning.WebApi/Controllers/v1/TrainingClassesController.cs`

| Method | Route | Permission |
|--------|-------|------------|
| `GET` | `/api/v1/training-classes` | `Classes.Read` |
| `GET` | `/api/v1/training-classes/{id}` | `Classes.Read` |
| `POST` | `/api/v1/training-classes` | `Classes.Create` |
| `PUT` | `/api/v1/training-classes/{id}` | `Classes.Update` |
| `DELETE` | `/api/v1/training-classes/{id}` | `Classes.Update` |
| `POST` | `/api/v1/training-classes/{id}/instructors` | `Classes.Update` |
| `DELETE` | `/api/v1/training-classes/{id}/instructors/{userId}` | `Classes.Update` |
| `POST` | `/api/v1/training-classes/{id}/sessions` | `Classes.ManageSessions` |
| `PUT` | `/api/v1/training-classes/{id}/sessions/{sessionId}` | `Classes.ManageSessions` |
| `POST` | `/api/v1/training-classes/{id}/sessions/{sessionId}/cancel` | `Classes.ManageSessions` |

**DTO example — schedule session**

```json
POST /api/v1/training-classes/{id}/sessions
{
  "title": "Week 1",
  "sessionType": "Zoom",
  "startUtc": "2026-04-10T01:00:00Z",
  "endUtc": "2026-04-10T03:00:00Z",
  "location": null
}
```

### Database

- **Migration**: `20260405121500_Sprint3_TrainingClassesAndSessions`
- **Tables**:
  - `training_classes` (FK `course_id` → `courses.id`)
  - `class_instructors` (FK `training_class_id` → `training_classes.id`, FK `user_id` → `users.id`)
  - `class_sessions` (FK `training_class_id` → `training_classes.id`)

## Tests

- **Domain**: `TrainingClassAggregateTests` (create class, schedule session rule, status transition)
- **Application**: `TrainingClassesFeatureSmokeTests` (validator rule for offline location)

## Deferred / follow-ups

- **Real Zoom integration**: replace `NoOpZoomMeetingService` with an implementation that calls Zoom REST APIs, handles auth/refresh, and stores meeting details robustly.
- **Webhooks**: Zoom webhook endpoints + idempotency (attendance typically belongs to Sprint 4+).
- **API integration tests** for training class routes.
- **EF Core tooling note**: `dotnet ef database update` may fail at design time due to existing `User._roles` primitive collection mapping; migration SQL can still be applied via DB tooling, or the design-time EF tooling setup can be aligned in a follow-up.

