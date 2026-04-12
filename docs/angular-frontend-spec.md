---
title: Angular — Spec, base components & state
scope: SRS template · reusable UI · state · Angular 18+ techniques
status: active
---

# Angular — requirements spec, reusable base components & state management

This document standardizes how to write **requirements specs** for the Angular app, **reusable base components**, **state management**, and recommended **Angular practices** for the ELearning project (planned frontend at `frontend/`, aligned with `api/v1` as described in `docs/api-design-guidelines.md`).

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

- Angular **standalone** components; do not add new `NgModule`-based features.
- TypeScript **strict** mode; path aliases (`@app/`, `@shared/`) in `tsconfig`.
- Style guide: **Angular style guide** + project ESLint rules.
- File naming: `feature-name.routes.ts`, `*.component.ts`, `*.store.ts` (when using a store).

### 1.6 Acceptance criteria

Use checklists verifiable manually or via E2E:

- [ ] Loading state appears when a request lasts longer than 300ms (or per UX).
- [ ] 4xx/5xx errors show user-friendly messages; 403 does not leak sensitive details.
- [ ] Invalid forms do not call the API; field-level errors are shown.

---

## 2. Folder structure (aligned with `docs/project-structure.md`)

Suggested layout beyond the minimal skeleton:

```
frontend/src/app/
├── core/                    # singletons, guards, interceptors, app config
│   ├── auth/
│   ├── http/
│   └── layout/
├── shared/                  # base components, pipes, directives, UI primitives
│   ├── ui/                  # button, input, card, dialog shell
│   ├── directives/
│   └── pipes/
└── features/                # lazy routes per business area
    ├── courses/
    └── dashboard/
```

**Rules**: `core` may import from `shared`, not the reverse. `features` import only `shared` and `core` (public APIs), not other features—except via `shared` or a shared domain library.

---

## 3. Reusable base components

### 3.1 Categories

| Type | Purpose | Examples |
|------|---------|----------|
| **Shell / layout** | Page frame, sidebar, router outlet | `AppShellComponent`, `PageHeaderComponent` |
| **Primitive UI** | No domain logic; display + `@Input` / `@Output` | `UiButtonComponent`, `UiCardComponent` |
| **Behavior / thin base** | DRY for unsubscribe, submit flows | `takeUntilDestroyed` or a small abstract class |
| **Abstract form** | Shared reactive form: submit, pending, error banner | `BaseFormComponent<T>` (optional) |

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

### 3.3 Presentational component — reference pattern

```typescript
// shared/ui/button/ui-button.component.ts
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-ui-button',
  standalone: true,
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || pending()"
      [attr.aria-busy]="pending()"
      (click)="clicked.emit()"
      class="ui-button"
    >
      @if (pending()) {
        <span class="ui-button__spinner" aria-hidden="true"></span>
      }
      <ng-content />
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UiButtonComponent {
  readonly type = input<'button' | 'submit'>('button');
  readonly disabled = input(false);
  readonly pending = input(false);
  readonly clicked = output<void>();
}
```

### 3.4 Page shell — reusable layout

```typescript
// shared/ui/page-shell/page-shell.component.ts
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  template: `
    <section class="page-shell">
      <header class="page-shell__header">
        <h1>{{ title() }}</h1>
        @if (subtitle()) {
          <p class="page-shell__subtitle">{{ subtitle() }}</p>
        }
      </header>
      <div class="page-shell__body">
        <ng-content />
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageShellComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
}
```

### 3.5 Content projection

- Use `ng-content` for free-form regions (actions, footer).
- For multiple slots, use named projection, e.g. `select="[shell-actions]"`, to keep templates readable.

---

## 4. State management

### 4.1 Layers

| Scope | Recommended tool | Notes |
|-------|------------------|-------|
| **Local UI state** | `signal`, `computed` | UI chrome, toggles |
| **Server / cache state** | `HttpClient` + signal store or **TanStack Query** (community) | Avoid duplicate requests; define refetch policy |
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

  // Prefer httpResource/rxResource when your Angular version supports them;
  // keep loading / error / data explicit at minimum.
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

**Next steps**: wrap HTTP with `httpResource` / `rxResource` when available for natural cancel/retry, or use `switchMap` + `takeUntilDestroyed`.

### 4.3 NgRx Signal Store (when needed)

Use when a feature has **many actions**, **complex derived state**, and reducer-style tests matter:

- Package: `@ngrx/signals`.
- Colocate the store with the feature (`features/courses/data/`).

### 4.4 Problem Details (`application/problem+json`)

- Map `title`, `detail`, and `status` from Problem Details to a small view model in an interceptor.
- Never show raw stack traces to end users.

---

## 5. Recommended Angular techniques

### 5.1 Application bootstrap

- **`bootstrapApplication`** + **`provideRouter`** with lazy `loadChildren`.
- **`provideHttpClient(withInterceptors([...]))`** — functional interceptors (Angular 15+).

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
- **`@defer`** for heavy below-the-fold content.

### 5.5 Forms

- **Reactive forms** for non-trivial forms; async validators when needed (e.g. unique email).
- **`ControlValueAccessor`** for custom controls such as `UiSelect`.

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
- Bundle analysis: `ng build --stats-json` + analyzer (per CLI version).

---

## 6. Initializing the frontend (when ready)

From the repo root (after creating `frontend/`):

```bash
cd frontend
npx @angular/cli@latest new . --routing --style=scss --ssr=false
```

Then apply the folder layout in section 2, add path aliases in `tsconfig.json`, and adopt the base component / store patterns from sections 3–4.

---

## 7. Related documentation

- `docs/project-structure.md` — `frontend/` skeleton
- `docs/api-design-guidelines.md` — REST, JWT, errors
- `docs/dotnet-backend-techniques.md` — backend behavior (CQRS, Problem Details)

---

## Revision history

| Date | Change |
|------|--------|
| 2026-04-12 | Initial SRS template, base components, state management, Angular 18+ practices, and code examples. |
| 2026-04-12 | Translated specification to English; trimmed unused import in store example. |
