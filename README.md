---
name: ELearning Plan
overview: Roadmap for a large-scale ELearning system with many entities/sub-entities, .NET Clean Architecture backend and Angular frontend, plus documentation updates.
todos:
  - id: init-structure
    content: Initialize BE/FE structure and toolchain
    status: pending
  - id: domain-uc
    content: Design domain model + core use cases
    status: pending
    dependencies:
      - init-structure
  - id: api-infra
    content: Build API, persistence, auth, audit/log, mapping
    status: pending
    dependencies:
      - domain-uc
  - id: fe-features
    content: Build FE modules by feature
    status: pending
    dependencies:
      - api-infra
  - id: tests-docs
    content: Add tests and update docs
    status: pending
    dependencies:
      - fe-features
---

<div align="center">

# ELearning Platform

A modern, large-scale **e-learning system** built with **.NET Clean Architecture** (backend) and **Angular** (frontend).

### Core stack

<a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET" /></a>
<a href="https://angular.dev/"><img src="https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular" /></a>
<a href="https://www.typescriptlang.org/"><img src="https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript" /></a>
<a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" /></a>

### Data & persistence

<a href="https://www.postgresql.org/"><img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" /></a>
<a href="https://learn.microsoft.com/en-us/ef/core/"><img src="https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Entity Framework Core" /></a>

### Libraries & cross-cutting

<a href="https://github.com/jbogard/MediatR"><img src="https://img.shields.io/badge/MediatR-68217A?style=flat-square&logo=dotnet&logoColor=white" alt="MediatR" /></a>
<a href="https://automapper.org/"><img src="https://img.shields.io/badge/AutoMapper-5C2D91?style=flat-square&logo=dotnet&logoColor=white" alt="AutoMapper" /></a>
<a href="https://serilog.net/"><img src="https://img.shields.io/badge/Serilog-1C1919?style=flat-square&logo=serilog&logoColor=white" alt="Serilog" /></a>
<a href="https://jwt.io/"><img src="https://img.shields.io/badge/JWT-000000?style=flat-square&logo=jsonwebtokens&logoColor=white" alt="JWT" /></a>
<a href="https://docs.fluentvalidation.net/"><img src="https://img.shields.io/badge/FluentValidation-059BED?style=flat-square" alt="FluentValidation" /></a>

</div>

## Main technologies

| Layer | Stack |
|--------|--------|
| **Backend** | .NET 8+ · Clean Architecture |
| **Frontend** | Angular 18+ · feature modules · Signals |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core |
| **Cross-cutting** | AutoMapper · Serilog · JWT · FluentValidation · MediatR |

---

## Scope

- Full: course management, students, instructors, enrollment, lesson content, quiz, certificates, payments, reports, notifications.
- Empty repo: start architecture and modules from scratch.

## High-level Architecture

- **Backend**: .NET (Clean Architecture) with `Domain` / `Application` / `Infrastructure` / `WebApi`.
- **Data & queries**: Generic Repository + UnitOfWork, AutoMapper + `ProjectTo` for query projection.
- **Observability**: Audit trail (who/when/what), structured logging, correlation id.
- **Frontend**: Angular (feature modules + shared + core).
- **Communication**: REST API (realtime notifications can be added later).

```mermaid
flowchart LR
  subgraph fe [AngularApp]
    ui[UIComponents]
    state[StateManagement]
    api[ApiClient]
  end
  subgraph be [DotNetApi]
    web[WebApi]
    app[Application]
    dom[Domain]
    infra[Infrastructure]
  end
  ui --> state --> api --> web
  web --> app --> dom
  app --> infra
  infra --> dom
```

## Proposed Folder Structure

- Backend
  - `backend/src/Domain`
  - `backend/src/Application`
  - `backend/src/Infrastructure`
  - `backend/src/WebApi`
- Frontend
  - `frontend/src/app/core`
  - `frontend/src/app/shared`
  - `frontend/src/app/features/*`
- Docs
  - `docs/architecture.md`
  - `docs/api.md`
  - `docs/frontend.md`

## Delivery Phases

### 1) Foundation

- Create .NET solution with Clean Architecture structure.
- Set up Angular workspace with `core/shared/features` modules.
- Configure code style, linting, basic CI.

### 2) Domain + Use Cases (BE)

- Core entities and sub-entities: Course, Section, Lesson, Instructor, Student, Enrollment, Quiz, Question, Attempt, Certificate, Payment, Notification, Report, Attachment, Progress, Review, Tag, Category.
- Value Objects: Money, Email, Duration, QuizScore, CourseStatus.
- Use cases: CRUD course, enrollment, publish course, assign instructor, take quiz, issue certificate, payment flow, reporting.

### 3) API + Infrastructure

- DbContext + migrations (SQL Server/Postgres).
- Generic Repository + UnitOfWork.
- Mapping: AutoMapper profiles + `ProjectTo` for optimized queries.
- REST endpoints by resource.
- AuthN/AuthZ (JWT + roles: Admin/Instructor/Student).
- Audit trail (created/updated/by, soft delete, change log).
- Logging (Serilog/OpenTelemetry), correlation id, error handling.
- Payment provider integration (Stripe/VNPay) via abstraction.
- Notification service (email + in-app).

### 4) Frontend Features

- Auth module: login/register/roles.
- Course management: list, detail, create/edit, publish.
- Learning: lesson viewer, progress tracking.
- Quiz: take quiz, view results.
- Certificates: view/download.
- Payments: checkout, history.
- Reports: dashboard/analytics.

### 5) Quality & Docs

- Unit/integration tests (BE), component tests (FE).
- Update docs: architecture, API contract, FE module map.

## Documentation to Update

- `docs/architecture.md`: Clean Architecture, data flow, dependency rules.
- `docs/api.md`: endpoints + request/response samples.
- `docs/frontend.md`: module structure, routing, state.
- `docs/data-access.md`: generic repository, UoW, mapping, `ProjectTo`, audit.

## Risks & Assumptions

- Assumed DB: Postgres (can be changed).
- Payment provider to be confirmed.

## Todos

- Initialize BE/FE structure and toolchain.
- Design domain model + core use cases.
- Build API + persistence + auth + audit/log/mapping.
- Build FE modules by feature.
- Add tests and update docs.
