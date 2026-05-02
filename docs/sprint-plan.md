---
title: Sprint Plan - ELearning LMS Project
scope: B2B+B2C Hybrid Learning Platform
methodology: Agile Scrum
sprint_duration: 2 weeks
team_size: 6-8 developers (3-4 BE, 2-3 FE, 1 DevOps)
status: in-progress
---

# Sprint Plan - ELearning LMS Project

## Project Overview

**Goal**: Build a production-grade B2B+B2C LMS with hybrid learning (Zoom + VOD), organization management, license pooling, commerce, and campaign features.

**Tech Stack**:
- Backend: .NET 10 (`net10.0`), Clean Architecture, EF Core, MediatR, FluentValidation
- Frontend: Angular 21, RxJS, NgRx (optional)
- Database: PostgreSQL
- Cache: Redis
- Messaging: RabbitMQ (optional)
- Infrastructure: Docker, Azure/AWS

**Team Structure**:
- 1 Project Manager / Scrum Master
- 1 Tech Lead / Architect
- 3-4 Backend Developers (.NET)
- 2-3 Frontend Developers (Angular)
- 1 DevOps Engineer
- 1 QA Engineer (optional)

---

## Current Progress Snapshot (Updated)

### Overall
- Sprint 0: **Partially Done** (Docker, local dev, baseline CI present; full quality gates / multi-env TBD)
- Sprint 1: **Done (backend + database + core tests)** — Angular UI tracked separately (see `frontend/README.md`, `docs/sprint1-completion.md`)
- Sprint 2: **In progress — backend core done** (course CRUD, sections/lessons, assets, migrations `Sprint2_CoursesAndContent`; cloud blob storage, sample seed, course UI **not done**)
- Sprint 3: **Backend MVP done** — `TrainingClass` aggregate, sessions, instructors, conflict checks, `IZoomMeetingService` stub; **real Zoom OAuth + webhooks + Angular UI** still open (see `docs/notice.md`)
- Sprint 4: **MVP done (Angular SPA)** — `frontend/web` Angular 19 app + Docker build; integrates auth, orgs, courses, training classes (see `docs/sprint4-completion.md`); Angular 21 upgrade optional; enrollment/attendance remains Sprint 5+
- **Sprint 4 polish (optional, 2–4 days)**: recommended before full Sprint 5 — thin `shared/ui` wrappers, pilot screen, global loading, E2E smoke, UX polish (see **Sprint 4b** below and `docs/sprint4-completion.md` *Sprint 4 review*)

### Completed Work Checklist
- [x] Backend solution skeleton in `src/` (Domain/Core/Application/Infrastructure/WebApi)
- [x] Core package setup (MediatR, FluentValidation, EF Core PostgreSQL, JWT, Redis, Hangfire, BCrypt)
- [x] Base architecture scaffolding (entities, result/error, repository, unit of work, DI)
- [x] Docker baseline (`docker-compose.yml`, API Dockerfile, frontend Dockerfile, nginx config)
- [x] Identity: Register, Login, Refresh Token, Get / Put profile
- [x] Organizations: create org, list orgs, get org + members, add member (Admin / OrgAdmin)
- [x] Admin: assign platform roles to users
- [x] EF migrations: `users`, `organizations`, `departments`, `organization_members`, **courses / sections / lessons / content assets**
- [x] Dev seed admin (Development only) via `DatabaseSeeder`
- [x] Role + permission authorization foundation
- [x] Security middleware baseline (exception handling, correlation ID)
- [x] Setup / security / sprint docs updated
- [x] **Sprint 2 (backend)**: Course aggregate, `CoursesController` API, local file storage via `IFileStorage`
- [x] **Sprint 3 (backend)**: Training class aggregate, `TrainingClassesController`, migration `Sprint3_TrainingClassesAndSessions`, `NoOpZoomMeetingService`
- [x] **Sprint 4 (frontend MVP)**: Angular app in `frontend/web`, auth/orgs/courses/training-classes UI, HTTP interceptors, Docker/nginx alignment

### Remaining Immediate Priorities
- [ ] **Frontend (recommended before Sprint 5)**: Sprint 4b polish — `UiButton` / `PageShell` / `UiDataTable`, pilot list screen, global loading indicator, minimal E2E, doc refresh (`docs/sprint4-completion.md`)
- [ ] **Sprint 3 follow-up**: real Zoom API implementation, webhooks, API integration tests for training classes
- [x] Angular SPA MVP: login, register, profile, orgs, courses, training classes (see `frontend/README.md`, `docs/sprint4-completion.md`)
- [ ] API integration tests (identity, organizations, **courses**)
- [ ] API rate limiting, lockout, and audit logging for auth actions
- [ ] CI/CD + Serilog sinks (carry-over from Sprint 0)
- [ ] Optional: S3/Azure Blob for `IFileStorage`, seed sample courses

### Execution Board (Owner + ETA)

| Task | Sprint Target | Owner | ETA | Status |
|---|---|---|---|---|
| EF migrations + org schema | Sprint 1 | Backend Team | — | **Done** |
| Seed initial admin (Development) | Sprint 1 | Backend Team | — | **Done** |
| Domain + application unit tests (baseline) | Sprint 1 | Backend + QA | — | **Done** |
| Course catalog + content API | Sprint 2 | Backend Team | — | **Done** |
| Class / session scheduling API | Sprint 3 | Backend Team | — | **Done (MVP)** |
| Angular SPA MVP (auth + org + courses + classes) | Sprint 4 | Frontend Team | 2 weeks | **MVP done** (see `docs/sprint4-completion.md`) |
| Sprint 4b — FE polish & wrappers (optional) | Pre–Sprint 5 | Frontend Team | 2–4 days | **Recommended** (see Sprint 4b) |
| API integration tests | Sprint 1–2 | Backend + QA | 2-3 days | Planned |
| API rate limiting + lockout + auth audit log | Sprint 1 | Backend + DevOps | 3-5 days | Planned |
| CI/CD + code quality pipeline | Sprint 0 (carry-over) | DevOps | 3-4 days | Planned |
| Serilog structured sink configuration | Sprint 0 (carry-over) | Backend + DevOps | 1-2 days | Planned |

### Sprint Completion %
- Sprint 0: **~70% complete** (core setup done, CI/CD and quality gates pending)
- Sprint 1: **~95% complete** (backend + DB + unit tests done; Angular UI optional follow-up)
- Sprint 2: **~75% complete** (backend + DB + unit/smoke tests; blob storage, sample seed, Angular course UI, API integration tests pending)
- Sprint 3: **~70% complete** (backend + DB + unit tests; real Zoom, webhooks, Angular UI, integration tests pending)
- Sprint 4: **MVP done**; stretch/deferred items tracked in **Sprint 4b** (wrappers, loading, E2E, section/lesson UI depth)

**Related docs**: `docs/notice.md` (triển khai — lưu ý kỹ thuật), `docs/dotnet-backend-techniques.md` (patterns backend).

---

## Sprint 0: Foundation & Setup (2 weeks)

**Goal**: Set up infrastructure, tooling, CI/CD, and project skeleton.

### Backend Tasks
- [x] Create .NET solution structure (Domain, Core, Application, Infrastructure, WebApi)
- [x] Set up EF Core + PostgreSQL connection
- [x] Configure Serilog structured logging
- [x] Set up MediatR + FluentValidation + AutoMapper
- [x] Create base entities (Entity, AggregateRoot, ValueObject)
- [x] Implement generic repository + UnitOfWork
- [x] Set up audit interceptor (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- [x] Implement soft delete interceptor
- [x] Configure JWT authentication
- [x] Set up OpenAPI docs with API versioning baseline
- [x] Create exception handling middleware
- [x] Set up correlation ID middleware

### Frontend Tasks
- **Status**: deferred / carry-over to **Sprint 4** (Angular workspace is not created yet; `frontend/` currently contains Docker/nginx + scaffold instructions only).
- [x] Create Angular workspace *(done in Sprint 4 as `frontend/web`; Angular 19 baseline due to local CLI 21 Node peer mismatch — see `docs/sprint4-completion.md`)*
- [x] Set up folder structure (core, shared, features) *(done in Sprint 4)*
- [x] Configure routing and lazy loading *(done in Sprint 4)*
- [x] Set up HTTP interceptors (auth, error) *(done in Sprint 4; loading indicator deferred)*
- [x] Create authentication service + guards *(done in Sprint 4)*
- [x] Set up environment configurations
- [ ] Create thin shared UI wrappers (e.g. `UiButton`, `PageShell`, `UiDataTable`) *(Sprint 4b; see `docs/spec/angular-frontend-spec.md` §8, `docs/sprint-plan.md` Sprint 4b)*
- [ ] Configure Tailwind CSS / Angular Material *(not used; PrimeNG adopted instead)*

### DevOps Tasks
- [ ] Set up Git repository + branching strategy
- [x] Configure Docker Compose (API, DB, Redis)
- [x] Set up CI/CD pipeline (GitHub Actions / Azure DevOps)
- [ ] Configure code quality tools (SonarQube, ESLint, Prettier)
- [ ] Set up development, staging, production environments

### Documentation
- [ ] Finalize architecture documentation
- [x] Create API design guidelines
- [ ] Set up Swagger documentation
- [x] Create developer onboarding guide

**Definition of Done**:
- Solution compiles and runs locally via Docker Compose
- CI/CD pipeline runs successfully
- Authentication works (login/register)
- Swagger documentation accessible
- Frontend connects to backend API

---

## Sprint 1: Identity & Organization Management (2 weeks) — **CLOSED (backend scope)**

**Goal**: Implement user management, roles, and organization (tenant) setup.

### Backend Tasks
- [x] **User aggregate**: Create User entity with roles (Admin, Instructor, Student)
- [x] **Organization aggregate**: Organization, Department, OrganizationMember entities
- [x] **Feature: Register user** (command + handler + validator)
- [x] **Feature: Login** (JWT token generation)
- [x] **Feature: Refresh token**
- [x] **Feature: Create organization** (B2B tenant) — `POST /api/v1/organizations` (Admin)
- [x] **Feature: Add member to organization** — `POST /api/v1/organizations/{id}/members`
- [x] **Feature: Assign roles to user** — `PUT /api/v1/users/{userId}/roles` (Admin)
- [x] Set up role-based authorization policies
- [x] Create user profile endpoints (GET `identity/me`, PUT `identity/me`)
- [x] Write unit tests for identity + organization domain + slug helper

### Frontend Tasks
- **Status**: deferred / carry-over to **Sprint 4** (backend scope closed; FE to integrate Sprint 1 APIs).
- [x] Create login page *(done in Sprint 4; PrimeNG)*
- [x] Create registration page *(done in Sprint 4; PrimeNG)*
- [x] Create user profile page *(done in Sprint 4; PrimeNG)*
- [x] Create organization management UI (admin) *(done in Sprint 4)*
- [x] Create member management UI *(done in Sprint 4)*
- [x] Implement role-based UI rendering *(done in Sprint 4)*
- [x] Add form validations *(done in Sprint 4)*

### Database
- [x] Create migrations for User, Organization, Department, OrganizationMember tables (`Sprint1_IdentityAndOrganizations`)
- [x] Seed initial admin user (Development only — `admin@localhost.local` / `ChangeMe123!` unless overridden)

**Definition of Done**:
- Users can register and login
- JWT tokens are issued and validated
- Organizations can be created
- Members can be added to organizations
- Role-based access control works
- All tests pass *(unit tests green; integration/E2E optional follow-up)*

---

## Sprint 2: Course Catalog & Content Management (2 weeks) — **BACKEND SCOPE DONE** *(follow-ups below)*

**Goal**: Build course catalog with sections, lessons, and content assets.

### Backend Tasks
- [x] **Course aggregate**: Course, Section, Lesson, ContentAsset entities
- [x] **Feature: Create course** (draft mode)
- [x] **Feature: Update course**
- [x] **Feature: Delete course** (soft delete)
- [x] **Feature: Publish course** (status change)
- [x] **Feature: Add section to course**
- [x] **Feature: Add lesson to section**
- [x] **Feature: Upload content asset** (video, PDF, SCORM)
- [x] **Feature: Get course details** (with sections/lessons)
- [x] **Feature: List courses** (paginated, filtered, sorted)
- [ ] Implement file upload service (S3 / Azure Blob) *(currently Local storage via `IFileStorage`)*
- [x] Write unit tests (domain + application validators / smoke)
- [ ] Write API-level integration tests for courses *(not present in `tests/` yet)*

### Frontend Tasks
- **Status**: deferred / carry-over to **Sprint 4** (backend scope done; FE to integrate Sprint 2 APIs).
- [x] Create course list page (with filters, search, pagination) *(done in Sprint 4; PrimeNG `p-table`)*
- [x] Create course detail page *(done in Sprint 4)*
- [x] Create course creation form *(done in Sprint 4; simplified create draft)*
- [ ] Create section/lesson management UI *(carry-over → Sprint 4; stretch)*
- [ ] Implement file upload component *(carry-over → Sprint 4; stretch)*
- [ ] Create rich text editor for lesson content *(carry-over → Sprint 4; stretch)*
- [ ] Add course preview mode *(carry-over → Sprint 4; stretch)*

### Database
- [x] Create migrations for Course, Section, Lesson, ContentAsset tables
- [ ] Seed sample courses

**Definition of Done**:
- [x] Courses can be created, updated, deleted
- [x] Sections and lessons can be managed
- [x] Content assets can be uploaded
- [x] Course catalog is browsable *(via API; Angular UI pending)*
- [x] Unit tests pass *(integration tests optional follow-up)*

---

## Sprint 3: Class Scheduling & Session Management (2 weeks) — **BACKEND MVP DONE** *(Zoom prod + UI + tests follow-up)*

**Goal**: Implement class (cohort) scheduling with Zoom and offline sessions.

### Backend Tasks
- [x] **Training class aggregate** (`TrainingClass`, `ClassSession`, `ClassInstructor` — tên tránh xung đột với keyword `class` trong C#)
- [x] **Feature: Create class** from published course — `POST /api/v1/training-classes`
- [x] **Feature: Schedule session** (Zoom/Offline/VOD) — `POST .../training-classes/{id}/sessions`
- [x] **Feature: Assign instructor to class** — `POST/DELETE .../instructors`
- [x] **Feature: Get class schedule** — `GET .../training-classes/{id}` (sessions ordered by time)
- [x] **Feature: Update session** — `PUT .../sessions/{sessionId}`
- [x] **Feature: Cancel session** — `POST .../sessions/{sessionId}/cancel`
- [x] Zoom integration via **`IZoomMeetingService`** + **`NoOpZoomMeetingService`** (placeholder URLs; replace for production Zoom API)
- [x] Instructor conflict detection (overlap across classes for assigned instructors)
- [x] Capacity: `max_learners` on `training_classes` (enforcement vs enrollment = Sprint 4)
- [x] Unit tests (domain + validator smoke)
- [ ] API integration tests for training classes

### Frontend Tasks
- **Status**: deferred / carry-over to **Sprint 4** (backend MVP done; FE to integrate Sprint 3 APIs).
- [x] Create class list page *(done in Sprint 4)*
- [x] Create class creation form *(done in Sprint 4)*
- [x] Create session scheduling UI *(done in Sprint 4 as a simple form + table)*
- [x] Create instructor assignment UI *(done in Sprint 4; minimal form)*
- [x] Display Zoom meeting links *(done in Sprint 4 when present)*
- [x] Create class detail page with schedule *(done in Sprint 4)*
- [x] Add conflict detection warnings *(done in Sprint 4; API errors surfaced in global banner)*

### Infrastructure
- [ ] Set up Zoom OAuth app *(required for production Zoom; not needed for `NoOp` stub)*
- [ ] Configure Zoom webhook endpoints *(attendance / meetings — often Sprint 4+)*

### Database
- [x] Migration `Sprint3_TrainingClassesAndSessions`: `training_classes`, `class_sessions`, `class_instructors`

**Definition of Done**:
- [x] Classes can be created from **published** courses
- [x] Sessions can be scheduled (Zoom/Offline/VOD)
- [x] Instructors can be assigned
- [x] Zoom-style meeting id/URL populated when type is Zoom *(dev: stub; prod: replace service)*
- [x] Schedule conflicts are detected for instructors
- [x] Unit tests pass *(integration tests optional follow-up)*

---

## Sprint 4: Frontend MVP (Angular) — Auth + Org + Courses + Classes (2 weeks)

**Goal**: Deliver the first usable Angular SPA that integrates with **already-delivered** backend APIs from Sprint 1–3 (identity/orgs/courses/training classes). Enrollment/attendance remains a follow-up sprint (see Sprint 5+).

### Scope (must-have)
- **Scaffold Angular app** under `frontend/web` and make it runnable locally (Angular **19** baseline; Angular 21 upgrade optional).
- **API integration** with the WebApi (base URL config, auth token handling, error handling).
- **Core user flows**:
  - Login + (optional) register
  - View/update profile
  - Organization list + organization detail/members (read + add member if API is ready/allowed)
  - Course list + course detail (read-first)
  - Training class list + detail (sessions) + schedule/update/cancel session (based on permissions)

### Frontend Tasks (checklist)
- [x] Scaffold Angular workspace + app (`frontend/web/`) — **Angular 19** baseline (CLI 21 upgrade optional; see `docs/sprint4-completion.md`)
- [x] App structure: `core/`, `shared/`, `features/` (standalone components, lazy routes)
- [x] Environment configuration: dev `apiUrl` → `http://localhost:5000`; prod empty → same-origin `/api` via nginx
- [x] PrimeNG UI baseline (PrimeNG 19 + PrimeIcons + animations) *(MVP UI library choice)*
- [x] PrimeNG theming via `definePreset` (`ELearningPreset` on Aura) *(theme tokens foundation; see `shared/ui/theme/elearning-preset.ts`)*
- [x] HTTP layer:
  - [x] Auth interceptor (attach JWT)
  - [x] Error interceptor (map Problem Details → global banner)
  - [x] Loading indicator (global) *(done in Sprint 4b: `loadingInterceptor` + top progress bar)*
- [x] Auth & session:
  - [x] Login + register + token persistence (`sessionStorage`)
  - [x] Route guards (`authGuard`, `guestGuard`)
  - [x] Profile page (GET/PUT `identity/me`)
- [x] Organizations (Sprint 1 API):
  - [x] Org list page
  - [x] Org detail page (members)
  - [x] Add member form (org role + user id)
  - [x] Create organization (Admin)
- [x] Courses (Sprint 2 API):
  - [x] Course list (search + pagination + status filter)
  - [x] Course detail
  - [x] Create draft course (Admin/Instructor)
- [x] Training classes (Sprint 3 API):
  - [x] Training class list + create (published course)
  - [x] Detail + sessions table
  - [x] Schedule / update / cancel session (conflicts surface via API → banner)
  - [x] Zoom join link when present
  - [x] Assign instructor (user id)
- [x] UX/quality:
  - [x] Basic layout + navigation
  - [x] Form validation + server error copy
  - [x] Canonical Angular/PrimeNG spec updated in `docs/spec/angular-frontend-spec.md` *(and `docs/angular-frontend-spec.md` stub redirect)*
  - [x] Minimal e2e smoke *(done in Sprint 4b: Playwright smoke test)*

### Backend/Infra Tasks (supporting, not the main deliverable)
- [x] CORS + API base URL: dev → `http://localhost:5000`; Docker UI → relative `/api` + nginx proxy (see `frontend/nginx.conf`)
- [ ] Ensure Swagger describes error shapes used by FE (validation/conflict/not found) *(carry-over)*

**Definition of Done**:
- [x] Angular app builds and runs locally (`frontend/web`).
- [x] Users can login and navigate core modules (orgs/courses/classes).
- [x] Training class sessions can be scheduled/updated/cancelled from the UI (given permissions); API errors shown in global banner.
- [x] No hardcoded API URLs (environment-based).
- [x] Happy path against local API documented in `docs/sprint4-completion.md`.

---

## Sprint 4b (optional): Polish & Foundation Strengthening (~2–4 days)

**Purpose**: Close high-value gaps from Sprint 4 **before** starting Sprint 5 (License Pool & B2B Management) so new features build on consistent UI patterns and less frontend debt.

**Rationale** (see `docs/spec/angular-frontend-spec.md` §8):

- First **`shared/ui`** wrappers (`UiButton`, `PageShell`, `UiDataTable`) establish conventions for loading, layout, and tables—Sprint 5 screens will reuse them.
- Global loading + minimal E2E raise confidence in regressions when adding B2B flows.
- Short focus window (2–4 days) is cheaper than retrofitting wrappers across many screens later.

### Checklist

- [x] Implement **`UiButton`**, **`PageShell`**, **`UiDataTable`** (table + paginator) in `shared/ui/`
- [x] Apply wrappers on **one pilot screen** (Course list)
- [x] **Global loading indicator** (HTTP activity) *(interceptor + top progress bar)*
- [ ] Minor **UX polish** on existing MVP pages (spacing, empty states, copy)
- [x] **Minimal E2E smoke** (login → courses) *(Playwright)*
- [x] Update **`docs/sprint4-completion.md`** when Sprint 4b items ship

**Explicitly still stretch / later** (not required for Sprint 4b): full **section/lesson management** UI, rich editor, file upload (see Sprint 2 FE stretch list).

**Sprint 5 entry**: Start Sprint 5 backend/FE work once Sprint 4b is done *or* explicitly skipped by team decision (record in sprint retro or this file).

---

## Sprint 5: License Pool & B2B Management (2 weeks)

**Goal**: Implement B2B license pooling and seat management.

### Backend Tasks
- [x] **LicensePool aggregate**: LicensePool, LicenseAssignment entities
- [x] **Feature: Create license pool** (org buys seats)
- [x] **Feature: Assign license to member**
- [x] **Feature: Revoke license**
- [x] **Feature: Get license usage report**
- [x] Enforce quota (prevent over-assignment) *(domain enforces seat availability)*
- [x] License expiry constraint *(domain prevents assignment if expired; expiry enforcement beyond that is follow-up)*
- [ ] **Feature: Bulk enroll via license**
- [ ] Create private class for organization
- [ ] Write unit + integration tests

### Frontend Tasks
- [x] Create license pool management UI (org admin) *(list + create)*
- [x] Create license assignment UI *(assign/revoke by user id in pool detail)*
- [x] Display license usage dashboard *(basic: seats used/available on detail screen)*
- [ ] Create member enrollment UI (org admin)
- [ ] Add license expiry warnings

### Database
- [ ] Create migrations for LicensePool, LicenseAssignment tables *(pending: EF tooling currently timing out in environment; schema defined via EF configurations)*

**Definition of Done**:
- Organizations can purchase license pools
- Licenses can be assigned to members
- Quota is enforced
- License usage is tracked
- All tests pass

---

## Sprint 6: Commerce & Pricing Engine (2 weeks)

**Goal**: Implement order, payment, and pricing engine.

### Backend Tasks
- [x] **Order aggregate**: Order, OrderItem + checkout expiry
- [x] **Feature: Create order** (cart → priced checkout server-side)
- [x] **Feature: Calculate price** *(MVP: prices on Course / TrainingClass / LicensePool + migration `Sprint6_PricingFields`)*
- [x] **Feature: Apply discount** (manual)
- [x] **Feature: Process payment** *(MVP: `IPaymentService` + `NoOpPaymentService`; Stripe/VNPay = provider swap + infra account)*
- [x] **Feature: Handle payment webhook** *(MVP: `/payments/webhook` + optional shared secret header)*
- [x] **Feature: Generate invoice** *(MVP: `invoices` row on successful payment)*
- [x] **Feature: Get order history** *(MVP: list buyer orders)*
- [x] Implement reservation pattern *(MVP: `checkout_reservations` for TrainingClass line items + capacity check vs other pending checkouts)*
- [x] Implement payment timeout *(MVP: `checkout_expires_at` + cancel on expiry during pay/webhook completion; 15 minutes)*
- [x] Write unit tests (domain) *(integration tests still deferred)*

### Frontend Tasks
- [x] Create course purchase flow *(published course / priced class / pool detail → `/checkout`; see `docs/sprint6-completion.md`)*
- [x] Create checkout page
- [ ] Integrate payment gateway UI *(prod: Stripe/VNPay; dev: NoOp “Pay now” on order detail)*
- [x] Create order confirmation *(order detail after place order; optional `?pay=1` auto-pay)*
- [x] Create order history page
- [x] Display invoice *(summary on order detail when status is Paid)*

### Infrastructure
- [ ] Set up Stripe/VNPay account *(production follow-up)*
- [x] Configure payment webhook endpoints *(MVP: `/api/v1/payments/webhook` + `Payments:WebhookSecret`)*

### Database
- [x] Create migrations for Order, OrderItem, pricing columns, payments, invoices, reservations *(see `Sprint6_*` migrations)*

**Definition of Done**:
- Users can purchase courses/classes *(Angular: detail pages → checkout → orders; API still authoritative)*
- Pricing is calculated correctly *(server reads catalog prices; ignores client-supplied unit prices)*
- Payments are processed *(NoOp MVP; replace provider for prod gateways)*
- Invoices are generated *(persisted invoice row)*
- Seat reservation works *(training-class checkout holds + timeout release)*
- All tests pass *(unit tests; integration tests optional follow-up)*

---

## Sprint 7: Campaign & Promotion Engine (2 weeks)

**Goal**: Implement campaign, coupon, and promotion rules.

### Backend Tasks
- [ ] **Campaign aggregate**: Campaign, PromotionRule, Coupon entities
- [ ] **Feature: Create campaign**
- [ ] **Feature: Apply campaign to order** (pricing engine integration)
- [ ] **Feature: Generate coupon codes**
- [ ] **Feature: Validate coupon**
- [ ] **Feature: Track campaign usage**
- [ ] Implement campaign eligibility rules
- [ ] Implement stacking rules (campaign + coupon)
- [ ] Implement volume discount (B2B)
- [ ] Implement usage limits (atomic update)
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create campaign management UI (admin)
- [ ] Create coupon input field (checkout)
- [ ] Display applied discounts
- [ ] Create campaign analytics dashboard
- [ ] Add campaign preview

### Database
- [ ] Create migrations for Campaign, PromotionRule, Coupon tables

**Definition of Done**:
- Campaigns can be created and managed
- Coupons can be generated and validated
- Discounts are applied correctly
- Usage limits are enforced
- Campaign analytics available
- All tests pass

---

## Sprint 8: Quiz & Assessment (2 weeks)

**Goal**: Implement quiz, questions, attempts, and grading.

### Backend Tasks
- [ ] **Quiz aggregate**: Quiz, Question, QuestionOption, Attempt, Score entities
- [ ] **Feature: Create quiz**
- [ ] **Feature: Add questions to quiz**
- [ ] **Feature: Submit quiz attempt**
- [ ] **Feature: Grade quiz** (auto + manual)
- [ ] **Feature: Get quiz results**
- [ ] **Feature: Get quiz analytics**
- [ ] Implement question types (MCQ, essay, code)
- [ ] Implement time limits
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create quiz creation UI (instructor)
- [ ] Create quiz-taking UI (student)
- [ ] Create grading UI (instructor, for essays)
- [ ] Display quiz results
- [ ] Create quiz analytics page

### Database
- [ ] Create migrations for Quiz, Question, Attempt, Score tables

**Definition of Done**:
- Quizzes can be created and assigned
- Students can take quizzes
- Auto-grading works for MCQ
- Manual grading available for essays
- Results are displayed
- All tests pass

---

## Sprint 9: Certificate & Completion (2 weeks)

**Goal**: Implement certificate issuance and course completion logic.

### Backend Tasks
- [ ] **Certificate aggregate**: Certificate, CertificateTemplate entities
- [ ] **Feature: Issue certificate** (on completion)
- [ ] **Feature: Get certificate**
- [ ] **Feature: Verify certificate** (public endpoint)
- [ ] Implement completion rules (attendance + progress + quiz)
- [ ] Generate certificate PDF
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create certificate template editor (admin)
- [ ] Display certificate (student)
- [ ] Create certificate download UI
- [ ] Create certificate verification page (public)
- [ ] Display completion status

### Infrastructure
- [ ] Set up PDF generation service

### Database
- [ ] Create migrations for Certificate, CertificateTemplate tables

**Definition of Done**:
- Certificates are issued on completion
- Certificates can be downloaded
- Certificates can be verified publicly
- All tests pass

---

## Sprint 10: Notifications & Messaging (2 weeks)

**Goal**: Implement in-app notifications, email, and messaging.

### Backend Tasks
- [ ] **Notification aggregate**: Notification, Message entities
- [ ] **Feature: Send notification** (in-app)
- [ ] **Feature: Send email** (via service)
- [ ] **Feature: Get user notifications**
- [ ] **Feature: Mark notification as read**
- [ ] **Feature: Send announcement** (course/class-wide)
- [ ] Implement notification templates
- [ ] Implement email templates
- [ ] Set up background job for notification delivery
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create notification bell UI
- [ ] Create notification list page
- [ ] Create announcement UI (instructor)
- [ ] Display real-time notifications (optional: SignalR)
- [ ] Create email preference settings

### Infrastructure
- [ ] Set up email service (SendGrid / AWS SES)
- [ ] Configure background job scheduler (Hangfire)

### Database
- [ ] Create migrations for Notification, Message tables

**Definition of Done**:
- In-app notifications work
- Emails are sent
- Announcements can be posted
- Notification preferences work
- All tests pass

---

## Sprint 11: Reporting & Analytics (2 weeks)

**Goal**: Implement dashboards and reports for admin, instructor, student.

### Backend Tasks
- [ ] **Feature: Get student dashboard** (enrolled classes, progress, upcoming sessions)
- [ ] **Feature: Get instructor dashboard** (classes, attendance, grades)
- [ ] **Feature: Get admin dashboard** (revenue, enrollments, active users)
- [ ] **Feature: Get course analytics** (enrollments, completion rate)
- [ ] **Feature: Get organization analytics** (license usage, member activity)
- [ ] **Feature: Export reports** (CSV, Excel)
- [ ] Implement caching for analytics queries
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create student dashboard
- [ ] Create instructor dashboard
- [ ] Create admin dashboard
- [ ] Create course analytics page
- [ ] Create organization analytics page
- [ ] Add charts and visualizations (Chart.js / D3.js)
- [ ] Implement report export

### Database
- [ ] Optimize queries for analytics (indexes, views)

**Definition of Done**:
- Dashboards are functional
- Analytics are accurate
- Reports can be exported
- Performance is acceptable
- All tests pass

---

## Sprint 12: Video On Demand (VOD) & Progress Tracking (2 weeks)

**Goal**: Implement video streaming, watch tracking, and completion logic.

### Backend Tasks
- [ ] **Video aggregate**: VideoAsset, WatchEvent entities
- [ ] **Feature: Upload video** (to S3/Azure/Mux)
- [ ] **Feature: Get video URL** (signed URL)
- [ ] **Feature: Track watch progress** (heartbeat)
- [ ] **Feature: Mark lesson complete** (watch threshold)
- [ ] Implement video transcoding (optional)
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Integrate video player (Video.js / Plyr)
- [ ] Implement watch tracking (heartbeat every 30s)
- [ ] Display video progress bar
- [ ] Auto-mark lesson complete at 80% watched
- [ ] Create video upload UI (instructor)

### Infrastructure
- [ ] Set up video storage (S3 / Azure Blob / Mux)
- [ ] Configure CDN for video delivery

### Database
- [ ] Create migrations for VideoAsset, WatchEvent tables

**Definition of Done**:
- Videos can be uploaded
- Videos are streamed via CDN
- Watch progress is tracked
- Lessons auto-complete at threshold
- All tests pass

---

## Sprint 13: Search & Filtering (2 weeks)

**Goal**: Implement full-text search and advanced filtering.

### Backend Tasks
- [ ] **Feature: Search courses** (full-text)
- [ ] **Feature: Filter courses** (category, price, level, instructor)
- [ ] **Feature: Sort courses** (popularity, rating, date)
- [ ] Implement Elasticsearch integration (optional)
- [ ] Optimize search queries
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create search bar (global)
- [ ] Create advanced filter UI
- [ ] Create search results page
- [ ] Implement faceted search
- [ ] Add search suggestions (autocomplete)

### Infrastructure
- [ ] Set up Elasticsearch (optional)

**Definition of Done**:
- Search works across courses
- Filters work correctly
- Search is performant
- All tests pass

---

## Sprint 14: Review & Rating (2 weeks)

**Goal**: Implement course reviews and ratings.

### Backend Tasks
- [ ] **Review aggregate**: Review, Rating entities
- [ ] **Feature: Submit review** (after completion)
- [ ] **Feature: Get course reviews** (paginated)
- [ ] **Feature: Calculate average rating**
- [ ] **Feature: Moderate reviews** (admin)
- [ ] Write unit + integration tests

### Frontend Tasks
- [ ] Create review submission form
- [ ] Display course reviews
- [ ] Display average rating (stars)
- [ ] Create review moderation UI (admin)

### Database
- [ ] Create migrations for Review, Rating tables

**Definition of Done**:
- Reviews can be submitted
- Ratings are displayed
- Average rating is calculated
- Reviews can be moderated
- All tests pass

---

## Sprint 15: Performance Optimization & Caching (2 weeks)

**Goal**: Optimize performance, add caching, and improve scalability.

### Backend Tasks
- [ ] Implement Redis caching for frequent queries
- [ ] Add response caching for public endpoints
- [ ] Optimize database queries (indexes, query analysis)
- [ ] Implement database query logging
- [ ] Add pagination to all list endpoints
- [ ] Implement rate limiting
- [ ] Add compression middleware
- [ ] Write performance tests

### Frontend Tasks
- [ ] Implement lazy loading for images
- [ ] Add virtual scrolling for long lists
- [ ] Optimize bundle size (tree shaking, code splitting)
- [ ] Add service worker for caching (PWA)
- [ ] Implement skeleton loaders

### Infrastructure
- [ ] Set up Redis cluster
- [ ] Configure CDN for static assets
- [ ] Set up load balancer

**Definition of Done**:
- API response time < 200ms (p95)
- Frontend load time < 3s
- Caching works correctly
- All tests pass

---

## Sprint 16: Security Hardening (2 weeks)

**Goal**: Implement security best practices and vulnerability fixes.

### Backend Tasks
- [ ] Implement CORS policy
- [ ] Add rate limiting per user/IP
- [ ] Implement request validation (anti-XSS, SQL injection)
- [ ] Add HTTPS enforcement
- [ ] Implement API key authentication (for webhooks)
- [ ] Add security headers (HSTS, CSP, X-Frame-Options)
- [ ] Implement audit logging for sensitive actions
- [ ] Run security scan (OWASP ZAP, SonarQube)
- [ ] Fix identified vulnerabilities

### Frontend Tasks
- [ ] Implement CSP headers
- [ ] Sanitize user inputs
- [ ] Add CSRF protection
- [ ] Implement secure token storage
- [ ] Add security headers

### Infrastructure
- [ ] Set up WAF (Web Application Firewall)
- [ ] Configure SSL/TLS certificates
- [ ] Set up secret management (Azure Key Vault / AWS Secrets Manager)

**Definition of Done**:
- Security scan passes
- All vulnerabilities fixed
- Security headers configured
- Audit logging works
- All tests pass

---

## Sprint 17: Mobile Responsiveness & Accessibility (2 weeks)

**Goal**: Ensure mobile-friendly UI and WCAG 2.1 AA compliance.

### Frontend Tasks
- [ ] Audit all pages for mobile responsiveness
- [ ] Fix mobile UI issues
- [ ] Implement responsive navigation
- [ ] Add touch-friendly interactions
- [ ] Run accessibility audit (Lighthouse, axe)
- [ ] Fix accessibility issues (ARIA labels, keyboard navigation, color contrast)
- [ ] Add screen reader support
- [ ] Test on multiple devices/browsers

**Definition of Done**:
- All pages are mobile-responsive
- Accessibility score > 90 (Lighthouse)
- WCAG 2.1 AA compliant
- All tests pass

---

## Sprint 18: Integration Testing & Bug Fixes (2 weeks)

**Goal**: End-to-end testing and bug fixing.

### Tasks
- [ ] Write E2E tests for critical user flows
- [ ] Run full regression testing
- [ ] Fix identified bugs
- [ ] Perform load testing
- [ ] Fix performance bottlenecks
- [ ] Update documentation

**Definition of Done**:
- All E2E tests pass
- No critical/high bugs
- Load test passes (1000 concurrent users)
- Documentation updated

---

## Sprint 19: User Acceptance Testing (UAT) (2 weeks)

**Goal**: Conduct UAT with stakeholders and fix feedback.

### Tasks
- [ ] Deploy to staging environment
- [ ] Conduct UAT sessions with stakeholders
- [ ] Collect feedback
- [ ] Prioritize feedback items
- [ ] Fix critical feedback items
- [ ] Re-test

**Definition of Done**:
- UAT sign-off received
- Critical feedback addressed
- System stable on staging

---

## Sprint 20: Production Deployment & Launch (2 weeks)

**Goal**: Deploy to production and launch.

### Tasks
- [ ] Finalize production environment setup
- [ ] Run final security audit
- [ ] Create deployment runbook
- [ ] Deploy to production
- [ ] Run smoke tests
- [ ] Monitor system health
- [ ] Prepare rollback plan
- [ ] Create user training materials
- [ ] Launch marketing campaign
- [ ] Monitor user feedback

**Definition of Done**:
- System live in production
- No critical issues
- Monitoring in place
- Support team trained
- Launch successful

---

## Post-Launch: Maintenance & Iteration

### Ongoing Tasks
- Monitor system health (uptime, performance, errors)
- Collect user feedback
- Prioritize feature requests
- Fix bugs
- Release updates (bi-weekly)
- Conduct retrospectives
- Update documentation

---

## Risk Management

| Risk | Impact | Mitigation |
|------|--------|------------|
| Zoom API changes | High | Monitor Zoom API changelog, implement adapter pattern |
| Payment gateway downtime | High | Implement fallback provider, queue payments |
| Database performance issues | Medium | Regular query optimization, add indexes, consider read replicas |
| Team member unavailability | Medium | Cross-train team, maintain documentation |
| Scope creep | Medium | Strict change control, prioritize MVP features |
| Security vulnerabilities | High | Regular security audits, automated scanning, bug bounty program |
| Third-party service failures | Medium | Implement circuit breakers, fallback mechanisms |

---

## Success Metrics

- **Technical**:
  - API uptime > 99.9%
  - API response time < 200ms (p95)
  - Frontend load time < 3s
  - Zero critical security vulnerabilities
  - Test coverage > 80%

- **Business**:
  - 1000+ registered users in first 3 months
  - 100+ courses published
  - 50+ organizations onboarded (B2B)
  - 10,000+ enrollments
  - 4.5+ average course rating

---

## Notes

- Sprint duration: 2 weeks
- Sprint planning: Day 1 of sprint
- Daily standup: Every day, 15 minutes
- Sprint review: Last day of sprint
- Sprint retrospective: Last day of sprint
- Backlog grooming: Mid-sprint

**Adjust sprint scope based on team velocity and priorities.**
