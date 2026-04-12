---
title: Angular — Spec, PrimeNG, base components & state
scope: SRS template · PrimeNG UI · state · Angular 18+ techniques
status: active
canonical: true
---

# Angular — requirements spec, PrimeNG UI, reusable patterns & state management

**Canonical copy:** this file (`docs/spec/angular-frontend-spec.md`) is the source of truth. A short stub at [`docs/angular-frontend-spec.md`](../angular-frontend-spec.md) points here so bookmarks and older links keep working.

This document standardizes how to write **requirements specs** for the Angular app, **PrimeNG-based UI**, **reusable patterns** (thin wrappers where they pay off), **state management**, and recommended **Angular practices** for the ELearning project (frontend at `frontend/web`, aligned with `api/v1` as described in `docs/api-design-guidelines.md`).

---

## 1. Software requirements specification (SRS) template for Angular

Use the structure below for each **epic / screen / user story** to keep BA, design, and engineering aligned.

### 1.1 Document header

| Field | Content |
|-------|---------|
| **ID** | `FE-XXX` or `USR-XXX` |
| **Document version** | Semver for the spec (e.g. `1.0.0`) |
| **Status** | Draft / Review / Approved |
| **API dependencies** | Endpoints, version (`api/v1/...`), contract (OpenAPI if available) |
| **User roles** | Learner, Instructor, Admin, … |

### 1.2 Scope & objectives

- **In scope**: UI behavior, navigation flows, client-side validation, loading / error / empty states.
- **Out of scope**: server-only business rules (state explicitly to avoid duplicating logic).

### 1.3 Functional requirements (numbered)

Write each requirement in **EARS** style (easy to test):

- **When** [condition / event],
- **the system** [response],
- **and** [displayed data / side effect].

Examples:

- **FR-01**: When the user opens the course list page, the system calls `GET /api/v1/courses` with the agreed query parameters and shows a skeleton state while waiting.
- **FR-02**: When the API returns 401, the system redirects to the login page and preserves `returnUrl` (query) so the user can return after signing in.

### 1.4 Non-functional requirements (NFR)

| Category | Suggested criteria |
|----------|-------------------|
| **Performance** | First load, LCP; lazy route per feature |
| **Accessibility** | WCAG 2.1 AA for primary flows; focus trap in dialogs |
| **Security** | Do not store refresh tokens in `localStorage` if policy forbids it; rely on host CSP headers |
| **i18n** | UI strings via `@angular/localize` or the team’s chosen i18n library |
| **Observability** | Correlation id header (align `X-Correlation-Id` with the backend) |

### 1.5 Technical conventions (bound to this repo)

- Angular **standalone** components; do not add new feature `NgModule`s. Import **PrimeNG modules or standalone components** per screen (e.g. `TableModule`, `Button`, `Card`) rather than registering everything globally.
- **UI library**: **PrimeNG** (version aligned with Angular major, e.g. **PrimeNG 19** with **Angular 19**). Theming via **`providePrimeNG`** + **`@primeuix/themes`** preset: base **Aura**, extended by project preset **`ELearningPreset`** (see §3.5). Icons: **PrimeIcons** (`primeicons` CSS in `styles.scss`).
- **Peer dependencies**: `@angular/animations`, **`@angular/cdk`** (required by PrimeNG).
- TypeScript **strict** mode; path aliases (`@core/*`, `@shared/*`) in `tsconfig.app.json`.
- Style guide: **Angular style guide** + **PrimeNG** usage patterns + project ESLint rules.
- File naming: `feature-name.routes.ts`, `*.component.ts`, `*.store.ts` (when using a store).
- Prefer **PrimeNG primitives** (`p-table`, `p-button`, `p-card`, `p-panel`, `p-dropdown`, `p-message`, `p-menubar`, `p-toolbar`, `p-paginator`, `p-tag`, `p-floatlabel`, `p-password`, …) in features. Introduce **`shared/ui/`** wrappers for repeated composition, loading/a11y conventions, or future design-system swaps (see §3.4).

### 1.6 Acceptance criteria

Use checklists verifiable manually or via E2E:

- [ ] Loading state appears when a request lasts longer than 300ms (or per UX).
- [ ] 4xx/5xx errors show user-friendly messages; 403 does not leak sensitive details.
- [ ] Invalid forms do not call the API; field-level errors are shown.

---

## 2. Folder structure (aligned with `docs/project-structure.md`)

Suggested layout beyond the minimal skeleton:

```
frontend/web/src/app/
├── core/                    # singletons, guards, interceptors, app config
│   ├── auth/
│   ├── http/
│   └── layout/
├── shared/                  # thin wrappers, pipes, directives (optional)
│   ├── ui/                  # theme preset, composed shells on top of PrimeNG
│   ├── directives/
│   └── pipes/
└── features/                # lazy routes per business area
    ├── courses/
    └── dashboard/
```

**Rules**: `core` may import from `shared`, not the reverse. `features` import only `shared` and `core` (public APIs), not other features—except via `shared` or a shared domain library.

---

## 3. Reusable UI (PrimeNG-first)

### 3.1 Categories

| Type | Purpose | Examples in this repo |
|------|---------|------------------------|
| **Shell / layout** | App chrome, navigation | `p-menubar` + `routerLink` in `MainLayoutComponent` |
| **Data display** | Tables, tags, empty states | `p-table`, `p-tag`, `p-message` |
| **Forms** | Inputs, validation UX | `p-floatlabel`, `pInputText`, `p-password`, `p-dropdown`, `p-inputNumber`, reactive or template-driven forms |
| **Feedback** | Global and inline errors | `p-message` (root banner + forms) |
| **Behavior / DRY** | RxJS cleanup, submit flows | `takeUntilDestroyed` (section 3.2); optional `BaseFormComponent<T>` only if many identical wizards |

### 3.2 Thin base class — avoid RxJS memory leaks

Prefer **`takeUntilDestroyed`** (Angular 16+) inside components; use an abstract base only when many screens repeat the same pattern.

```typescript
// shared/util/with-destroy.ts — optional helper; prefer takeUntilDestroyed inline
import { DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export function injectDestroyRef(): DestroyRef {
  return inject(DestroyRef);
}

// In a component:
// source$.pipe(takeUntilDestroyed()).subscribe(...);
```

### 3.3 Feature component — reference pattern (PrimeNG + standalone)

Import only what the route needs. Example: login uses `p-card`, `p-floatlabel`, `p-password`, and `p-button`.

```typescript
// features/auth/login.component.ts (excerpt)
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, Card, FloatLabel, InputText, Password, Button],
  template: `
    <p-card header="Sign in">
      <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-column gap-3">
        <p-floatlabel>
          <input pInputText id="email" formControlName="email" fluid class="w-full" />
          <label for="email">Email</label>
        </p-floatlabel>
        <p-password formControlName="password" [feedback]="false" inputId="pw" [fluid]="true" />
        <p-button type="submit" label="Sign in" [loading]="pending()" [disabled]="form.invalid" />
      </form>
    </p-card>
  `,
})
export class LoginComponent {
  /* ... */
}
```

Use **`pTemplate`** (import `PrimeTemplate` from `primeng/api`) for `p-table`, `p-toolbar`, and `p-menubar` slots.

### 3.4 When to add `shared/ui/*` (wrappers)

- **Do not** recreate buttons, tables, or dialogs by hand unless PrimeNG cannot meet an accessibility or UX requirement.
- **Do** add a **thin wrapper** when the same **composition or behavior** repeats (loading + `aria-busy`, table + paginator + sort defaults, standard page chrome). Wrappers stay **PrimeNG-first**: they delegate to `p-*` components and encode **team conventions** only.

**Planned high-level wrappers** (introduce incrementally; 2–3 per sprint is enough):

| Component | Wraps / composes | Conventions to enforce |
|-----------|------------------|-------------------------|
| `UiButtonComponent` | `p-button` | Unified `loading` / `pending`, sizes, `aria-busy` / `aria-disabled` where applicable |
| `UiCardComponent` / `UiPanelComponent` | `p-card` / `p-panel` | Optional header slots, consistent padding / severity |
| `UiDataTableComponent` | `p-table` + `p-paginator` | Default sort/pagination model, `OnPush`, shared empty/loading slots |
| `UiInputComponent` | float label + text input | Label `for` / `id`, error text association |
| `PageShellComponent` | layout regions | Title, actions slot, main content; can wrap PrimeFlex / toolbar patterns |

**Rollout**: pick one list screen and migrate it to `UiDataTableComponent` (or `UiButton` only) first; then replicate. Avoid big-bang refactors.

### 3.5 Theming and design tokens (`definePreset`)

- Theme is registered in `app.config.ts` via **`providePrimeNG({ theme: { preset: ELearningPreset, ... } })`**.
- **Brand / semantic tokens** live in a single preset file so we avoid large global CSS overrides (recommended for PrimeNG v19+ / styled mode).

```typescript
// frontend/web/src/app/shared/ui/theme/elearning-preset.ts
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const ELearningPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{sky.50}',
      100: '{sky.100}',
      200: '{sky.200}',
      300: '{sky.300}',
      400: '{sky.400}',
      500: '{sky.500}',
      600: '{sky.600}',
      700: '{sky.700}',
      800: '{sky.800}',
      900: '{sky.900}',
      950: '{sky.950}',
    },
  },
});
```

Adjust palettes (`sky`, `indigo`, custom hex via token maps) when design finalizes. Use `@primeuix/themes` helpers such as `updatePrimaryPalette` / `updateSurfacePalette` if the team prefers palette-oriented edits.

- Global icon font: `@import 'primeicons/primeicons.css';` in `src/styles.scss`.
- Prefer PrimeNG **severity** (`p-tag`, `p-message`) and **built-in spacing** utilities; keep custom SCSS minimal.

---

## 4. State management

### 4.1 Layers

| Scope | Recommended tool | Notes |
|-------|------------------|-------|
| **Local UI state** | `signal`, `computed` | UI chrome, toggles |
| **Server / cache state** | `HttpClient` + signal store, **`httpResource` / `rxResource`**, or **TanStack Query** (`@tanstack/angular-query-experimental`) | Avoid duplicate requests; define refetch policy |
| **Session / user** | `inject` + `AuthStore` service (signals) | JWT claims, current org |
| **Cross-screen shared state** | **NgRx Store** or **NgRx SignalStore** | Many subscribers, devtools / time-travel |

**Principle**: do not put the entire app in NgRx when most flows are CRUD + HTTP caching; start with **signals + scoped services** and adopt NgRx when complexity warrants it.

### 4.2 Injectable store with signals (baseline pattern)

```typescript
// features/courses/data/course-list.store.ts
import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface CourseSummary {
  id: string;
  title: string;
}

@Injectable()
export class CourseListStore {
  private readonly http = inject(HttpClient);

  readonly filter = signal('');

  readonly items = signal<CourseSummary[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly filtered = computed(() => {
    const q = this.filter().trim().toLowerCase();
    const list = this.items();
    if (!q) return list;
    return list.filter((c) => c.title.toLowerCase().includes(q));
  });

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<CourseSummary[]>('/api/v1/courses').subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: (e: Error) => {
        this.error.set(e.message);
        this.loading.set(false);
      },
    });
  }
}
```

### 4.3 Data-heavy screens (tables, pagination, CRUD lists)

For screens that use **`p-table`** heavily (courses, organizations, training classes, admin lists), prefer one of:

- **`httpResource` / `rxResource`** (Angular **19.2+**): built-in loading / error / value signals, request cancellation when inputs change, less `subscribe` + `takeUntilDestroyed` boilerplate for straightforward GETs.
- **TanStack Query for Angular**: caching, deduplication, background refetch, pagination helpers—strong fit when many routes share the same API entities or you want stale-while-revalidate UX.

Keep **simple signal stores** for local filters and UI-only state; compose with resources/queries where HTTP lifecycle is non-trivial.

### 4.4 NgRx Signal Store (when needed)

Use when a feature has **many actions**, **complex derived state**, and reducer-style tests matter:

- Package: `@ngrx/signals`.
- Colocate the store with the feature (`features/courses/data/`).

### 4.5 Problem Details (`application/problem+json`)

- Map `title`, `detail`, and `status` from Problem Details to a small view model in an interceptor.
- Never show raw stack traces to end users.

---

## 5. Recommended Angular techniques

### 5.1 Application bootstrap

- **`bootstrapApplication`** + **`provideRouter`** with lazy `loadChildren`.
- **`provideHttpClient(withInterceptors([...]))`** — functional interceptors (Angular 15+).
- **`provideAnimationsAsync()`** — expected by PrimeNG overlays and motion.
- **PrimeNG** — register once in `app.config.ts`:

```typescript
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import { ELearningPreset } from './shared/ui/theme/elearning-preset';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: ELearningPreset,
        options: { darkModeSelector: false },
      },
    }),
    // ... provideRouter, provideHttpClient, etc.
  ],
};
```

**Dependencies** (see `frontend/web/package.json`): `primeng`, `@primeuix/themes`, `primeicons`, `@angular/animations`, `@angular/cdk`.

### 5.2 Routing

- **Lazy load** each feature to reduce the initial bundle.
- **Functional guards**, e.g. `canActivate: [authGuard]`, instead of class-based guards when possible.
- **Title & meta**: `title` in route data or the `Title` service.

### 5.3 HTTP

- **Base URL** from `environment.apiUrl`.
- **Interceptors**: Bearer token, correlation id, 401 handling (refresh or redirect per backend policy).
- **Typed clients**: response interfaces match backend DTOs.

### 5.4 Templates

- **`@if` / `@for` / `@switch`** instead of `*ngIf` / `*ngFor` once the team agrees.
- **`@defer`** for heavy **below-the-fold** blocks (large tables, secondary panels) to improve LCP; pair with triggers (`on viewport`, `on idle`) per UX.

### 5.5 Forms

- **Reactive forms** for non-trivial forms; async validators when needed (e.g. unique email). Template-driven **`ngModel`** is acceptable for small admin-only panels (e.g. organization create) when it keeps templates short.
- PrimeNG inputs (`pInputText`, `p-password`, `p-dropdown`, …) work with both reactive and template-driven forms.
- **`ControlValueAccessor`** only if wrapping a non-PrimeNG widget or a highly custom control.

### 5.6 Change detection

- Default **`ChangeDetectionStrategy.OnPush`** for new components.
- Update state with **signals** or **immutable data** to avoid subtle bugs.

### 5.7 Testing

- **Jest** or **Karma** (per CLI template).
- Components: `TestBed` + `HttpClientTestingModule`.
- E2E: Playwright or Cypress — smoke login + one primary CRUD flow.

### 5.8 Build & quality

- **Strict templates** (`strictTemplates: true`).
- **ESLint** `@angular-eslint`, **Prettier** if the team uses it.
- **Bundle analysis**: `ng build --stats-json` + analyzer. PrimeNG is **heavy**—keep **lazy routes**, **per-component imports**, and revisit **`angular.json` budgets** when adding modules. Prefer deferring non-critical UI and avoiding unused PrimeNG imports.

### 5.9 Accessibility (PrimeNG)

- Every input: visible **label** associated via `for` / `id` (float label still needs stable `id` on the control).
- **Buttons**: discernible name (label text or `aria-label` for icon-only).
- **Tables**: scope or headers for complex grids; don’t rely on color alone for status (use `p-tag` + text).
- **Loading**: expose **busy** state (`aria-busy` on submit buttons / regions where appropriate).
- **Focus**: visible focus ring (theme tokens); trap focus in dialogs when using `p-dialog`.

---

## 6. Initializing the frontend (when ready)

The repo already contains an app under **`frontend/web`** (Sprint 4). To recreate from scratch elsewhere:

```bash
cd frontend
npx @angular/cli@<version> new elearning-web --directory=web --routing --style=scss --ssr=false
```

Then install UI dependencies and apply the folder layout in section 2:

```bash
cd frontend/web
npm install primeng @primeuix/themes primeicons @angular/animations @angular/cdk
```

Wire **`providePrimeNG`** / **`provideAnimationsAsync`** as in section 5.1, import **PrimeIcons** in `styles.scss`, add **`ELearningPreset`** (or start from raw Aura), and adopt patterns from sections 3–4.

---

## 7. Related documentation

- `docs/project-structure.md` — `frontend/` skeleton
- `docs/api-design-guidelines.md` — REST, JWT, errors
- `docs/dotnet-backend-techniques.md` — backend behavior (CQRS, Problem Details)

---

## 8. Roadmap & ownership (next sprint)

Suggested **priority** (incremental; no big-bang):

1. **Wrappers**: Ship **2–3** of `UiButton`, `PageShell`, or `UiDataTable`; migrate **one** feature as the reference implementation.
2. **Theming**: Iterate **`ELearningPreset`** with design (primary palette, typography) using `definePreset` / palette helpers.
3. **Data layer**: Pilot **`httpResource`** or TanStack Query on **one** list screen; document the chosen pattern in this spec if we standardize.

**Sync**: a short planning call helps align **priority order** and **owners** (who lands wrappers vs. table migration vs. tokens). If async is enough, capture decisions in `docs/sprint-plan.md` and tick items in sprint completion notes.

---

## Revision history

| Date | Change |
|------|--------|
| 2026-04-12 | Initial SRS template, base components, state management, Angular 18+ practices, and code examples. |
| 2026-04-12 | Translated specification to English; trimmed unused import in store example. |
| 2026-04-12 | Sprint 4: documented existing app path `frontend/web`; init commands updated. |
| 2026-04-12 | **PrimeNG**: spec and `frontend/web` use PrimeNG 19 + Aura; UI refactored to `p-*` components; budgets adjusted for library size. |
| 2026-04-12 | **Canonical spec** under `docs/spec/`; **`ELearningPreset`** (`definePreset` on Aura); roadmap for wrappers, `httpResource`/TanStack, `@defer`, bundle and a11y notes. |
