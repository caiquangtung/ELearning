# Frontend (Sprint 1+)

The production UI is planned as **Angular 21** (standalone components, signals, lazy routes). This folder currently holds **Docker/nginx** assets for serving a static build.

## Prerequisites

- **Node.js** ≥ 20.19 or ≥ 22.12 (required by Angular CLI 21).
- **npm** 10+.

Verify before scaffolding:

```bash
node -v
npx @angular/cli@21 version
```

## Scaffold (when ready)

From the repository root:

```bash
cd frontend
npx @angular/cli@21 new web --directory=web --routing --style=scss --ssr=false
```

Then wire the app to the API base URL (e.g. `https://localhost:7xxx` or reverse-proxied `/api`), add HTTP client, auth interceptor, and routes aligned with `docs/sprint-plan.md` Sprint 1 frontend backlog.

## Build for Docker

After `ng build --configuration production`, copy `dist/web/browser` (or your output path) into the image expected by `frontend/Dockerfile` / `docker-compose.yml`.
