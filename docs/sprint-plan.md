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
- Sprint 9: **Backend MVP done** — certificate aggregate, issue/get/verify/download APIs, completion rule validation, PDF generation, EF migration `Sprint9_Certificates`; Angular certificate UI still open (see `docs/sprint9-completion.md`)
- Sprint 10: **MVP done** — notification/message aggregate, in-app notification APIs, announcements, email service abstraction, unread count endpoint, Angular notification bell/list/announcement UI, EF migration `Sprint10_NotificationsMessaging`; delivery templates/background jobs/Redis cache, realtime notifications, and preferences still open (see `docs/sprint10-completion.md`)
- Sprint 11: **MVP done** — reporting read-model service, admin/student/instructor dashboard APIs, course/organization analytics APIs, Angular dashboard KPI cards; export, Redis analytics cache, chart library, and integration tests still open (see `docs/sprint11-completion.md`)
- Sprint 12: **MVP done** — video asset/watch progress aggregates, upload/playback/progress/complete APIs, EF migration `Sprint12_VideoProgress`, Angular course lesson video player/upload/progress tracking; CDN/transcoding and production video storage still open (see `docs/sprint12-completion.md`)
- Sprint 13: **MVP done** — course catalog search across title/description/lessons, status/price filters, sort options, global course search, and course results UI; category/level/instructor facets, Redis catalog cache, suggestions, and Elasticsearch still open (see `docs/sprint13-completion.md`)
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
- [x] **Sprint 9 (backend MVP)**: Certificate aggregate, issue/get/verify APIs, completion rules, migration `Sprint9_Certificates`
- [x] **Sprint 10 (MVP)**: Notifications/messages aggregate, in-app notifications, announcements, unread counts, `IEmailService`/`NoOpEmailService`, Angular notification bell/list/announcement UI, migration `Sprint10_NotificationsMessaging`
- [x] **Sprint 11 (MVP)**: Admin/student/instructor dashboard APIs, course/organization analytics APIs, Angular dashboard KPI cards
- [x] **Sprint 12 (MVP)**: Video upload/playback/progress tracking, watch completion threshold, course lesson video UI, migration `Sprint12_VideoProgress`
- [x] **Sprint 13 (MVP)**: Course search/filter/sort API, global course search, advanced course list filters

### Remaining Immediate Priorities
- [ ] **Frontend (recommended before Sprint 5)**: Sprint 4b polish — `UiButton` / `PageShell` / `UiDataTable`, pilot list screen, global loading indicator, minimal E2E, doc refresh (`docs/sprint4-completion.md`)
- [x] **Redis performance layer**: add cache, distributed lock, idempotency, and rate-limit abstractions before broad analytics/search/AI rollout (see **Sprint 15a**)
- [ ] **Sprint 10 follow-up**: notification templates, background delivery, Redis unread-count cache, realtime notifications, email preference settings
- [ ] **Sprint 11 follow-up**: CSV/Excel export, Redis analytics cache, dedicated course/org analytics pages, chart library visualizations, reporting integration tests
- [ ] **Sprint 12 follow-up**: S3/Azure/Mux storage, CDN delivery, transcoding, richer player UX, reporting integration with completion rules
- [ ] **Sprint 13 follow-up**: category/level/instructor course metadata, Redis catalog cache, search suggestions, Elasticsearch, dedicated faceted search page
- [ ] **AI Sprint Track (after Sprint 13 foundation)**: add AI-assisted quiz generation, course recommendation, essay grading assistant, learner risk prediction, and semantic search (see **AI Sprint Track** below)
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
| AI-1 — Quiz question generator | AI Sprint Track | Backend + Frontend | 2 weeks | Planned |
| AI-2 — Course recommendation | AI Sprint Track | Backend + Data | 2 weeks | Planned |
| AI-3 — Essay grading assistant | AI Sprint Track | Backend + Frontend | 2 weeks | Planned |
| AI-4 — Learner risk prediction | AI Sprint Track | Backend + Data | 2 weeks | Planned |
| AI-5 — Semantic search + learning path generator | AI Sprint Track | Backend + Frontend | 2 weeks | Planned |
| Sprint 15a — Redis performance & consistency layer | Pre–Sprint 15 | Backend + DevOps | 1 week | Done |
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

## AI Sprint Track: AI-Assisted Learning Features (5 sprints)

**Goal**: Add practical AI capabilities that demonstrate NLP, recommendation, prediction, and AI service integration while keeping human review and auditability in the LMS workflow.

**Recommended placement**: Start after Sprint 13, when quiz, course content, analytics/progress, and basic search data are available. AI-1 can start earlier after Sprint 8 if the team wants an early AI demo.

**AI Platform Baseline**
- [ ] Add `IAiService` / `ILlmService` abstraction in Application/Core and provider implementation in Infrastructure.
- [ ] Configure provider options through environment variables (`Provider`, `ApiKey`, `Model`, timeout, retry policy).
- [ ] Add prompt templates with version IDs for audit and repeatability.
- [ ] Store AI request metadata: user, feature, input hash, model, prompt version, token/cost estimate, created time.
- [ ] Add guardrails: content size limits, profanity/sensitive-data filtering where applicable, structured JSON response validation, graceful fallback when provider fails.
- [ ] Add permissions for instructor/admin AI actions and rate limits for AI endpoints.

### AI-1: Quiz Question Generator (Priority 1, 2 weeks)

**Goal**: Let instructors generate draft quiz questions from course/lesson content, then review before saving.

#### Backend Tasks
- [ ] **Feature: Generate quiz questions** — `POST /api/v1/ai/quizzes/generate-questions`
- [ ] Input: `courseId`, `lessonId`, `questionCount`, `difficulty`, `questionTypes`
- [ ] Output structured draft questions: text, type, options, correct answers, explanation, difficulty
- [ ] Validate generated JSON before returning to UI
- [ ] Add “accept generated question” flow that reuses existing quiz question commands
- [ ] Persist AI generation metadata for audit
- [ ] Unit tests for prompt builder, response parser, validation, and failure fallback

#### Frontend Tasks
- [ ] Add “Generate with AI” action in quiz create/detail screen
- [ ] Add review panel for generated questions before insertion
- [ ] Allow instructor to edit, discard, or accept each generated question
- [ ] Show provider failure as a recoverable UI state

**Definition of Done**:
- Instructor can generate 5-10 draft questions from a lesson
- Generated questions are never saved until explicitly accepted
- Invalid provider responses are rejected safely
- Audit metadata is recorded for each generation

### AI-2: Course Recommendation (Priority 2, 2 weeks)

**Goal**: Recommend courses to learners based on profile, organization context, course history, and semantic similarity.

#### Backend Tasks
- [ ] **Feature: Get learner course recommendations** — `GET /api/v1/ai/recommendations/courses`
- [ ] Implement hybrid scoring: role/department rules, popularity, completion history, quiz performance, and course similarity
- [ ] Add explainable recommendation reasons (`Because you completed...`, `Popular in your department...`)
- [ ] Add fallback recommendations when learner history is sparse
- [ ] Add tests for score ranking and tenant isolation

#### Frontend Tasks
- [ ] Add “Recommended for you” section to learner dashboard
- [ ] Add recommendation cards to course catalog
- [ ] Display concise recommendation reasons

**Definition of Done**:
- Learner receives ranked, explainable course recommendations
- Recommendations respect organization boundaries and published-course status
- Empty-history users still receive sensible fallback recommendations

### AI-3: Essay Grading Assistant (Priority 3, 2 weeks)

**Goal**: Assist instructors with rubric-based essay grading while keeping final grading human-controlled.

#### Backend Tasks
- [ ] **Feature: Suggest essay grades** — `POST /api/v1/ai/quizzes/attempts/{attemptId}/grade-suggestions`
- [ ] Input: essay answers, question text, max score, optional rubric
- [ ] Output: suggested score, confidence, reasoning, rubric breakdown
- [ ] Add guardrail: AI suggestion cannot submit final grade directly
- [ ] Record accepted/overridden suggestions for audit and model-quality review
- [ ] Unit tests for authorization, response parsing, and manual override behavior

#### Frontend Tasks
- [ ] Add AI suggestion panel to manual grading screen
- [ ] Let instructor apply, edit, or ignore suggested score
- [ ] Show rubric explanation without hiding the learner answer

**Definition of Done**:
- Instructor can request AI suggestions for essay answers
- Final grade is still submitted through the existing grading workflow
- Accepted vs overridden suggestions are auditable

### AI-4: Learner Risk Prediction (Priority 4, 2 weeks)

**Goal**: Predict learners at risk of not completing a course or license assignment and suggest interventions.

#### Backend Tasks
- [ ] **Feature: Get learner risk** — `GET /api/v1/ai/learners/{userId}/risk`
- [ ] **Feature: Organization risk report** — `GET /api/v1/ai/organizations/{organizationId}/risk-report`
- [ ] Implement explainable scoring from progress, quiz score, inactivity, attendance, license expiry, and class timeline
- [ ] Return `riskScore`, `riskLevel`, `reasons`, and `recommendedActions`
- [ ] Add scheduled risk snapshot job for B2B reporting
- [ ] Tests for scoring thresholds, data isolation, and missing-data behavior

#### Frontend Tasks
- [ ] Add risk badges to organization learner report
- [ ] Add risk detail drawer with reasons and recommended actions
- [ ] Add filter for high-risk learners

**Definition of Done**:
- Organization admin can see high-risk learners with reasons
- Risk model is explainable and deterministic for MVP
- Missing progress data does not produce misleading high-risk labels

### AI-5: Semantic Search + Learning Path Generator (Priority 5, 2 weeks)

**Goal**: Improve discovery with semantic search and generate draft learning paths from learner goals.

#### Backend Tasks
- [ ] **Feature: Semantic course search** — `GET /api/v1/ai/search/courses?q=...`
- [ ] Generate and refresh course embeddings for published courses
- [ ] Implement vector similarity with fallback keyword search
- [ ] **Feature: Generate learning path** — `POST /api/v1/ai/learning-paths/generate`
- [ ] Input: learner goal, current skills, target role, optional organization scope
- [ ] Output: ordered courses with reasons and estimated effort
- [ ] Tests for embedding refresh, ranking, fallback, and tenant boundaries

#### Frontend Tasks
- [ ] Add semantic search mode to course catalog search
- [ ] Add “Create learning path with AI” entry point
- [ ] Show generated path as draft that admin/learner can edit before saving

**Definition of Done**:
- Natural-language queries can find semantically related courses
- AI-generated paths are drafts, not automatically assigned
- Search and path generation respect published status and organization visibility

### AI Track Risks & Controls
- [ ] **Cost control**: rate limit AI endpoints, log token/cost estimates, and cache stable outputs where appropriate.
- [ ] **Reliability**: every AI feature must have a non-AI fallback or recoverable UI state.
- [ ] **Privacy**: do not send unnecessary PII to the provider; hash or omit learner identifiers in prompts.
- [ ] **Quality**: use structured outputs and validation; do not persist invalid AI responses.
- [ ] **Human control**: generated questions, essay grades, and learning paths remain drafts until a user accepts them.

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
- [x] **Campaign aggregate**: Campaign, PromotionRule, Coupon entities *(see `docs/sprint7-completion.md`)*
- [x] **Feature: Create campaign**
- [x] **Feature: Apply campaign to order** (pricing engine integration)
- [x] **Feature: Generate coupon codes** *(manual code creation in MVP)*
- [x] **Feature: Validate coupon** *(checkout quote endpoint)*
- [x] **Feature: Track campaign usage** *(coupon redemptions + analytics endpoint)*
- [x] Implement campaign eligibility rules *(MVP: window + scope)*
- [x] Implement stacking rules (campaign + coupon) *(MVP: best discount across eligible global/org + coupon campaign)*
- [x] Implement volume discount (B2B) *(MVP: license pool quantity tiers)*
- [x] Implement usage limits (atomic update) *(coupon usage reservations with TTL)*
- [x] Write unit + integration tests *(unit tests added; integration tests deferred)*

### Frontend Tasks
- [x] Create campaign management UI (admin)
- [x] Create coupon input field (checkout)
- [x] Display applied discounts *(checkout quote summary)*
- [x] Create campaign analytics dashboard *(MVP: analytics panel on campaign detail)*
- [x] Add campaign preview *(admin: campaign detail “Preview (quote)” panel)*

### Database
- [x] Create migrations for Campaign, PromotionRule, Coupon tables

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
- [x] **Quiz aggregate**: Quiz, Question, QuestionOption, Attempt, Score entities
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

## Sprint 9: Certificate & Completion (2 weeks) — **BACKEND MVP DONE** *(PDF + UI follow-up)*

**Goal**: Implement certificate issuance and course completion logic.

### Backend Tasks
- [x] **Certificate aggregate**: Certificate, CertificateTemplate entities
- [x] **Feature: Issue certificate** (on completion)
- [x] **Feature: Get certificate**
- [x] **Feature: Verify certificate** (public endpoint)
- [x] Implement completion rules (attendance + progress + quiz)
- [x] Generate certificate PDF
- [x] Write unit tests
- [ ] Write integration tests

### Frontend Tasks
- [ ] Create certificate template editor (admin)
- [ ] Display certificate (student)
- [ ] Create certificate download UI
- [ ] Create certificate verification page (public)
- [ ] Display completion status

### Infrastructure
- [x] Set up PDF generation service

### Database
- [x] Create migrations for Certificate, CertificateTemplate tables

**Definition of Done**:
- Certificates are issued on completion
- Certificates can be downloaded
- Certificates can be verified publicly
- All tests pass

---

## Sprint 10: Notifications & Messaging (2 weeks)

**Goal**: Implement in-app notifications, email, and messaging.

### Backend Tasks
- [x] **Notification aggregate**: Notification, Message entities
- [x] **Feature: Send notification** (in-app)
- [x] **Feature: Send email** (via service)
- [x] **Feature: Get user notifications**
- [x] **Feature: Mark notification as read**
- [x] **Feature: Send announcement** (course/class-wide)
- [ ] Implement notification templates
- [ ] Implement email templates
- [ ] Set up background job for notification delivery
- [ ] Use Redis cache for unread notification counts (`notifications:unread:{userId}`)
- [ ] Optional: use Redis Pub/Sub or SignalR backplane for multi-instance real-time notifications
- [x] Write unit tests
- [ ] Write integration tests

### Frontend Tasks
- [x] Create notification bell UI
- [x] Create notification list page
- [x] Create announcement UI (instructor)
- [ ] Display real-time notifications (optional: SignalR)
- [ ] Create email preference settings

### Infrastructure
- [ ] Set up email service (SendGrid / AWS SES)
- [ ] Configure background job scheduler (Hangfire)

### Database
- [x] Create migrations for Notification, Message tables

**Definition of Done**:
- In-app notifications work
- Emails are sent
- Announcements can be posted
- Notification preferences work
- Unread counters do not require a database aggregate query on every page load
- All tests pass

---

## Sprint 11: Reporting & Analytics (2 weeks)

**Goal**: Implement dashboards and reports for admin, instructor, student.

### Backend Tasks
- [x] **Feature: Get student dashboard** (MVP: paid orders, purchases, certificates, upcoming purchased-class sessions)
- [x] **Feature: Get instructor dashboard** (MVP: assigned classes and scheduled/past sessions)
- [x] **Feature: Get admin dashboard** (MVP: revenue, users, courses, classes, certificates, checkout)
- [x] **Feature: Get course analytics** (MVP: classes, certificates, paid course-order revenue)
- [x] **Feature: Get organization analytics** (MVP: members, license seats, paid org-order revenue)
- [ ] **Feature: Export reports** (CSV, Excel)
- [ ] Implement Redis caching for analytics queries with short TTL and explicit invalidation on key writes
- [ ] Cache dashboard cards: admin, instructor, student, course analytics, organization analytics
- [ ] Add cache-key conventions for tenant-safe analytics (`analytics:{scope}:{id}:{version}`)
- [ ] Write unit + integration tests

### Frontend Tasks
- [x] Create student dashboard
- [x] Create instructor dashboard
- [x] Create admin dashboard
- [ ] Create course analytics page
- [ ] Create organization analytics page
- [x] Add dashboard KPI visualizations (MVP cards; Chart.js / D3.js deferred)
- [ ] Implement report export

### Database
- [ ] Optimize queries for analytics (indexes, views)

**Definition of Done**:
- Dashboards are functional
- Analytics are accurate
- Reports can be exported
- Redis-backed analytics cache keeps repeated dashboard loads performant
- All tests pass

---

## Sprint 12: Video On Demand (VOD) & Progress Tracking (2 weeks)

**Goal**: Implement video streaming, watch tracking, and completion logic.

### Backend Tasks
- [x] **Video aggregate**: VideoAsset, WatchEvent entities
- [x] **Feature: Upload video** *(MVP: local storage via existing `IFileStorage`; S3/Azure/Mux deferred)*
- [x] **Feature: Get video URL** *(MVP: local asset URL with range support; signed CDN URL deferred)*
- [x] **Feature: Track watch progress** (heartbeat)
- [x] **Feature: Mark lesson complete** (watch threshold)
- [ ] Implement video transcoding (optional)
- [x] Write unit tests
- [ ] Write integration tests

### Frontend Tasks
- [x] Integrate video player *(MVP: native HTML5 player)*
- [x] Implement watch tracking (heartbeat every 30s)
- [x] Display video progress indicator
- [x] Auto-mark lesson complete at 80% watched
- [x] Create video upload UI (instructor)

### Infrastructure
- [ ] Set up video storage (S3 / Azure Blob / Mux)
- [ ] Configure CDN for video delivery

### Database
- [x] Create migrations for VideoAsset, WatchEvent tables

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
- [x] **Feature: Search courses** *(MVP: title, description, lesson title/content using database query; full-text index deferred)*
- [x] **Feature: Filter courses** *(MVP: status + price range; category/level/instructor metadata deferred)*
- [x] **Feature: Sort courses** *(MVP: newest, oldest, title, price; popularity/rating deferred)*
- [ ] Use Redis cache for published course catalog, course detail, filter metadata, and frequent search results
- [ ] Invalidate catalog cache on course create/update/publish/delete and review/rating changes
- [ ] Implement Elasticsearch integration (optional)
- [x] Optimize search queries *(MVP: server-side filtering/sorting/pagination; DB full-text indexes deferred)*
- [x] Write unit tests
- [ ] Write integration tests

### Frontend Tasks
- [x] Create search bar (global)
- [x] Create advanced filter UI
- [x] Create search results page *(MVP: Courses page with query params)*
- [x] Implement faceted search *(MVP: status + price + sort controls)*
- [ ] Add search suggestions (autocomplete)

### Infrastructure
- [ ] Set up Elasticsearch (optional)

**Definition of Done**:
- Search works across courses
- Filters work correctly
- Search is performant
- Course catalog cache is invalidated correctly when course visibility/content changes
- All tests pass

---

## Sprint 14: Review & Rating (2 weeks) - Completed

**Goal**: Implement course reviews and ratings.

### Backend Tasks
- [x] **Review aggregate**: Review/rating fields and moderation status
- [x] **Feature: Submit review** (after completion)
- [x] **Feature: Get course reviews** (paginated)
- [x] **Feature: Calculate average rating**
- [x] **Feature: Moderate reviews** (admin)
- [x] Write unit + integration tests

### Frontend Tasks
- [x] Create review submission form
- [x] Display course reviews
- [x] Display average rating (stars)
- [x] Create review moderation UI (admin)

### Database
- [x] Create migration for Review/rating data

**Definition of Done**:
- Reviews can be submitted
- Ratings are displayed
- Average rating is calculated
- Reviews can be moderated
- All tests pass

---

## Sprint 15a: Redis Performance & Consistency Layer (1 week) - Completed

**Goal**: Add reusable Redis-backed infrastructure for high-read paths, concurrency control, idempotency, and rate limiting before broad optimization work.

### Backend Tasks
- [x] Add `ICacheService` abstraction with get/set/remove/get-or-create helpers and JSON serialization.
- [x] Add cache key conventions and tenant-safe key builder.
- [x] Add `IDistributedLockService` for short-lived locks around checkout, seat reservation, coupon usage, license assignment, and background jobs.
- [x] Add `IIdempotencyStore` for payment webhook event IDs and payment completion attempts.
- [x] Add Redis-backed rate limit primitives for auth, checkout, upload, and AI endpoints.
- [x] Add cache invalidation hooks for course create/update/publish/delete.
- [x] Add Redis health check and startup configuration validation.
- [x] Add structured logs/metrics for cache hit/miss, lock acquisition failure, idempotency duplicate, and rate-limit rejection.
- [x] Unit tests for key generation, TTL behavior, idempotency duplicate detection, and lock failure behavior.

### Application Integration Targets
- [x] Course catalog/detail cache: `courses:list:{hash}`, `courses:detail:{courseId}`.
- [x] Dashboard analytics cache: `analytics:{scope}:{id}:{hash}`.
- [x] Payment webhook idempotency: `payment:webhook:{gateway}:{eventId}`.
- [x] Checkout and seat lock: `lock:checkout:{userId}`, `lock:class-seat:{classId}`.
- [x] Coupon lock: `lock:coupon:{couponCode}`.
- [x] License pool lock: `lock:license-pool:{poolId}`.
- [ ] Permission cache: `permissions:user:{userId}` with invalidation on role assignment.
- [ ] AI cost/rate cache: `rate:ai:{userId}`, `ai:recommendations:{userId}`, `ai:semantic-search:{queryHash}`.

### Infrastructure Tasks
- [x] Verify local Redis in `docker-compose.yml` is production-like enough for development.
- [x] Add environment variables for Redis connection, default TTLs, lock TTLs, and rate-limit windows.
- [x] Document Redis key naming, TTL policy, and invalidation strategy.

**Definition of Done**:
- Shared Redis abstractions are available to Application/Infrastructure without leaking provider-specific APIs into feature handlers.
- Payment webhooks are idempotent across retries.
- Checkout/license/coupon operations can use short-lived distributed locks while keeping database constraints as the source of truth.
- Course catalog and analytics have measurable cache hit/miss logging.
- Rate limit primitives are ready for auth, checkout, uploads, and AI endpoints.

---

## Sprint 15: Performance Optimization & Caching (2 weeks) - Completed

**Goal**: Optimize performance, add caching, and improve scalability.

### Backend Tasks
- [x] Implement Redis caching for frequent queries
- [x] Add response caching for public endpoints
- [x] Optimize database queries (indexes, query analysis)
- [x] Implement database query logging
- [x] Add pagination to all list endpoints
- [x] Implement rate limiting
- [x] Add compression middleware
- [x] Write performance tests

### Frontend Tasks
- [x] Implement lazy loading for images *(no current catalog image grids; covered by static asset cache/PWA policy until image-heavy screens are added)*
- [x] Add virtual scrolling for long lists
- [x] Optimize bundle size (tree shaking, code splitting) *(existing route-level lazy loading verified; no new PWA package dependency added)*
- [x] Add service worker for caching (PWA)
- [x] Implement skeleton loaders

### Infrastructure
- [x] Set up Redis cluster *(runbook/template added; actual environment provisioning remains deployment work)*
- [x] Configure CDN for static assets *(runbook/template added)*
- [x] Set up load balancer *(Nginx template added)*

**Definition of Done**:
- API response time < 200ms (p95)
- Frontend load time < 3s
- Caching works correctly
- All tests pass

---

## Sprint 16: Security Hardening (2 weeks) - Completed

**Goal**: Implement security best practices and vulnerability fixes.

### Backend Tasks
- [x] Implement CORS policy
- [x] Add rate limiting per user/IP
- [x] Implement request validation (anti-XSS, SQL injection)
- [x] Add HTTPS enforcement
- [x] Implement API key authentication (for webhooks)
- [x] Add security headers (HSTS, CSP, X-Frame-Options)
- [x] Implement audit logging for sensitive actions
- [x] Run security scan (OWASP ZAP, SonarQube) *(local/CI script added; ZAP runs when target is configured)*
- [x] Fix identified vulnerabilities *(non-breaking runtime fixes applied; dev-tooling residuals documented where Angular CLI 21 would be required)*

### Frontend Tasks
- [x] Implement CSP headers
- [x] Sanitize user inputs
- [x] Add CSRF protection
- [x] Implement secure token storage
- [x] Add security headers

### Infrastructure
- [x] Set up WAF (Web Application Firewall) *(runbook/template added)*
- [x] Configure SSL/TLS certificates *(runbook/template added)*
- [x] Set up secret management (Azure Key Vault / AWS Secrets Manager) *(runbook/template added)*

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
