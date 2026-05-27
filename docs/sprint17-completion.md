# Sprint 17 Completion: Mobile Responsiveness & Accessibility

Completed on 2026-05-27.

## Scope

- Added responsive mobile app navigation with a sticky app bar, drawer, keyboard Escape close, notification access, global search, and skip-to-content support.
- Improved global responsive defaults: focus-visible ring, reduced-motion handling, touch-friendly targets, wrapping toolbars, scroll-safe data tables, and mobile layout spacing.
- Hardened shared Angular UI wrappers for accessibility: `PageShell`, `UiButton`, and `UiDataTable` now expose better aria labels, content landmarks, and keyboard-focusable table scroll regions.
- Updated core course, training class, organization, and quiz list surfaces for mobile-friendly filters/forms and clearer labels.
- Adjusted the PrimeNG primary token to a darker shade so primary buttons meet WCAG AA contrast in axe checks.
- Added Playwright + axe coverage for login, register, and mocked authenticated mobile navigation.

## Verification

- `npm run build` passed with the existing initial bundle budget warning (`734.31 kB` vs `700 kB` warning budget).
- `npm run e2e -- accessibility.spec.ts` passed: 3 tests, including WCAG A/AA axe checks and 390px mobile navigation.

## Residual Risks

- Full Lighthouse scoring was not run in this workspace; keep it as a staging or local browser QA gate before release.
- Existing smoke E2E still depends on a real API and seeded credentials; Sprint 17 added mocked accessibility/mobile coverage rather than replacing that flow.
- Some PrimeNG toggleable panels still rely on PrimeNG internals; future work should replace high-risk toggleable admin forms with first-party accessible disclosure components if axe flags those authenticated admin paths.
