---
title: Sprint 4 completion — Angular SPA MVP
status: Frontend MVP delivered (Angular 19; upgrade to Angular 21 optional)
---

## Goal

Deliver a first usable SPA that integrates with Sprint 1–3 APIs: **identity**, **organizations**, **courses**, and **training classes** (read + key write flows), per `docs/sprint-plan.md` Sprint 4.

## Delivered

### Workspace

- **Path**: `frontend/web/` — Angular CLI app **`elearning-web`** (Angular **19.2**; scaffolded with `@angular/cli@19` because CLI 21 reported a Node version mismatch in the local `npx` environment).
- **Docker**: `frontend/Dockerfile` builds from `web/`; output `dist/elearning-web/browser` → nginx. `frontend/.dockerignore` excludes `web/node_modules` and `web/dist`.
- **Environments**:
  - **Development** (`environment.development.ts`): `apiUrl: 'http://localhost:5000'` (host API from `docker compose` or local `dotnet run`).
  - **Production** (`environment.ts`): `apiUrl: ''` — browser uses relative `/api/v1/...`; `frontend/nginx.conf` proxies `/api` → `http://api:8080`.

### Architecture

- **Standalone** components, **lazy-loaded** feature routes.
- **UI**: **PrimeNG 19** (`primeng`, `@primeuix/themes` **Aura** extended by **`ELearningPreset`** in `shared/ui/theme/elearning-preset.ts`, `primeicons`); `providePrimeNG` + `provideAnimationsAsync` in `app.config.ts`.
- **Folders**: `src/app/core`, `src/app/shared`, `src/app/features`.
- **HTTP**: `provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))`.
  - **Auth**: attaches `Authorization: Bearer <accessToken>` from `sessionStorage`.
  - **Errors**: maps API Problem Details (`detail` / `title`) to `GlobalErrorService`; **401** on non-login/register clears session and redirects to `/login` with a session message (except login failure, which shows server message).
- **API client**: `core/api/lms-api.service.ts` — typed calls for organizations, courses, training classes, sessions, instructors.

### User flows

| Flow | Notes |
|------|--------|
| **Login / Register** | `POST /api/v1/identity/login`, `register`; tokens + user JSON in `sessionStorage`. |
| **Profile** | `GET` / `PUT /api/v1/identity/me`. |
| **Organizations** | List + detail + **add member** (user GUID + org role). **Create organization** (name, optional slug) when role is **Admin**. |
| **Courses** | Paginated list (search, status filter), **create draft** (Admin/Instructor), detail with sections/lessons/assets. |
| **Training classes** | Paginated list, **create** from **Published** course (Admin/OrgAdmin/Instructor), detail with sessions table, **schedule/update/cancel** session, **Zoom join** link, **assign instructor** by user id. |

### Configuration snippets

**Path aliases** (`tsconfig.app.json`):

```json
"baseUrl": ".",
"paths": {
  "@core/*": ["./src/app/core/*"],
  "@shared/*": ["./src/app/shared/*"]
}
```

**Dev API base** (`frontend/web/src/environments/environment.development.ts`):

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
};
```

## How to run (quick)

```bash
# Terminal 1 — API + DB
docker compose up api postgres redis

# Terminal 2 — Angular
cd frontend/web && npm install && npm start
```

Browse `http://localhost:4200`, sign in with seeded admin (`docs/developer-onboarding.md`).

## Deferred / follow-ups

- Upgrade to **Angular 21** when local/CI Node + `npx @angular/cli@21` align with project policy.
- **Refresh token** rotation in the interceptor (currently 401 → logout).
- **E2E** smoke (Playwright/Cypress): login → courses → class detail.
- Richer **org/course/class** UX (wizard, calendar, file upload).
- **NgRx / SignalStore** if global client state grows (`docs/spec/angular-frontend-spec.md`).
- **`shared/ui` wrappers**, **`httpResource`/TanStack** for heavy tables, **`@defer`** / bundle tuning — see roadmap in `docs/spec/angular-frontend-spec.md` §8.

## Revision

| Date | Change |
|------|--------|
| 2026-04-12 | Initial Sprint 4 MVP: scaffold, core HTTP/auth, orgs/courses/classes UI, Docker + docs. |
| 2026-04-12 | **PrimeNG 19** + Aura theme; lists use `p-table`, forms use `p-card` / `p-floatlabel` / `p-dropdown`, shell uses `p-menubar`. |
| 2026-04-12 | **ELearningPreset** (`definePreset` on Aura); canonical Angular spec moved to `docs/spec/angular-frontend-spec.md` with wrapper/theming/state roadmap. |
