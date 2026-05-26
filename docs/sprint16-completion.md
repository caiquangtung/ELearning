# Sprint 16 Completion: Security Hardening

Completed on 2026-05-25.

## Scope

- Hardened CORS parsing/startup validation and removed credentialed CORS for the Bearer-token flow.
- Added security headers and unsafe request origin validation middleware.
- Replaced inline payment webhook secret checks with a reusable constant-time webhook secret filter.
- Added request-size limits to webhook and upload endpoints.
- Added persisted audit logs for sensitive authentication, payment, role, review, license, and campaign actions.
- Centralized frontend auth storage behind an Angular service while keeping `sessionStorage`.
- Added local/CI security scan script and production security runbook.
- Updated Angular 19 patch versions and npm lockfile to apply non-breaking audit fixes.

## Verification

- `dotnet test src/ELearning.sln --no-restore -m:1 /nr:false` passed.
- `npm run build` passed with the existing initial bundle budget warning.
- `bash scripts/security-scan.sh` passed; ZAP baseline was skipped because `ZAP_TARGET` was not set.

## Residual Risks

- HttpOnly-cookie auth and full CSRF token flow are deferred by decision.
- WAF, TLS certificates, and secret-manager resources require environment provisioning outside this repository.
- Full npm dev-tooling audit still reports Angular CLI/build-chain transitive advisories that require a breaking CLI 21 upgrade or have no non-breaking fix; production dependency audit is clean.
