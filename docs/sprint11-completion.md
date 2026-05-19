---
title: Sprint 11 completion - Reporting & Analytics
status: MVP delivered
date: 2026-05-19
scope: Backend + Frontend MVP
---

# Sprint 11 Completion - Reporting & Analytics

## Delivered

- Reporting read model:
  - Added `IReportingReadService` in Application and EF-backed `ReportingReadService` in Infrastructure.
  - Kept reporting logic out of controllers and out of the domain aggregates.
- Dashboard APIs:
  - `GET /api/v1/reports/dashboard/admin`
  - `GET /api/v1/reports/dashboard/student`
  - `GET /api/v1/reports/dashboard/instructor`
- Analytics APIs:
  - `GET /api/v1/reports/courses/{courseId}`
  - `GET /api/v1/reports/organizations/{organizationId}`
- Admin dashboard MVP:
  - Users, active users, courses, published courses, classes, scheduled classes, paid/pending orders, revenue, and certificates issued.
- Student dashboard MVP:
  - Paid orders, course purchases, class purchases, certificates, and upcoming sessions from purchased classes.
- Instructor dashboard MVP:
  - Assigned classes, upcoming sessions, past scheduled sessions, draft classes, and scheduled classes.
- Organization analytics MVP:
  - Member count, license pool count, total seats, active seats, paid org orders, and revenue.
- Course analytics MVP:
  - Class count, certificate count, paid course-order count, and revenue.
- Frontend:
  - Replaced the static Angular dashboard with role-aware KPI cards.
  - Admin users load platform dashboard metrics.
  - Learners load student metrics.
  - Instructors load teaching metrics in addition to student metrics.

## Deferred

- CSV/Excel report export.
- Redis-backed analytics cache and tenant-safe cache key conventions.
- Dedicated course analytics and organization analytics pages.
- Chart.js/D3 visualizations beyond KPI cards.
- Query indexes/views specifically tuned for reporting.
- API integration tests for reporting endpoints.

## Verification

- `dotnet test src/ELearning.sln` passes:
  - Domain unit tests: 34 passed.
  - Application unit tests: 16 passed.
  - Architecture tests: 1 passed.
- `npm run build` passes for `frontend/web`.

## Notes

- Dashboard metrics are direct database read-model queries for MVP. Redis caching should be added with the Sprint 15a cache abstraction rather than hard-coded inside reporting handlers.
- Student dashboard uses paid orders and issued certificates because enrollment/progress tracking is not fully modeled yet.
