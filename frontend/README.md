# Frontend — Angular SPA (Sprint 4+)

The **ELearning** UI is an **Angular 19** application under `web/` (**PrimeNG 19** + **Aura** theme via `@primeuix/themes`, standalone components, lazy routes, signals). Docker builds from this folder and serves static files with nginx; `/api` is reverse-proxied to the API container (see `nginx.conf`).

## Prerequisites

- **Node.js** ≥ 20.19 or ≥ 22.12 (Angular 19 CLI runs on the toolchain used in CI/Docker).
- **npm** 10+.

```bash
node -v
cd web && npx ng version
```

## Local development

1. Start the API (e.g. `docker compose up api postgres redis` or `dotnet run` for `ELearning.WebApi` on port **5000**).
2. CORS must allow `http://localhost:4200` (see `appsettings.Development.json` → `Cors:AllowedOrigins`).
3. From `frontend/web`:

```bash
npm install
npm start
```

Open `http://localhost:4200`. The dev build uses `src/environments/environment.development.ts` → `apiUrl: 'http://localhost:5000'`.

Sign in with the seeded admin (Development): `admin@localhost.local` / `ChangeMe123!` (unless overridden in config).

## Production build (host)

```bash
cd web
npm run build
```

Output: `web/dist/elearning-web/browser/`.

## Docker image

From repo root:

```bash
docker compose build frontend
```

The image runs `npm ci` + `ng build` inside `frontend/` (see `Dockerfile`). Production bundle uses `environment.ts` with **`apiUrl: ''`** so the browser calls same-origin **`/api/v1/...`**, which nginx proxies to the API service.

## Project layout (`web/src/app`)

| Area | Purpose |
|------|---------|
| `core/` | Auth service, guards, HTTP interceptors, `LmsApiService`, global error banner |
| `shared/layout/` | Authenticated shell (nav + `router-outlet`) |
| `features/` | Login/register, dashboard, profile, organizations, courses, training classes |

See also: `docs/sprint-plan.md` (Sprint 4), `docs/sprint4-completion.md`, `docs/spec/angular-frontend-spec.md` (canonical Angular spec; `docs/angular-frontend-spec.md` redirects).
