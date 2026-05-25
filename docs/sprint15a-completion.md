# Sprint 15a Completion: Redis Performance & Consistency Layer

Completed on 2026-05-24.

## Scope

- Added Redis-backed shared abstractions for cache, key building, distributed locks, idempotency, and rate-limit counters.
- Added lazy Redis connection handling so cache reads fail open when Redis is unavailable.
- Added Redis health check on `/health`.
- Added cache for course list/detail queries and dashboard/analytics queries.
- Added invalidation for course list/detail cache on course create/update/delete/publish and review moderation.
- Added payment webhook idempotency keyed by provider and transaction id.
- Added distributed locks for checkout, training class seat reservations, coupon usage, and license pool assignment.

## Redis Policy

- Cache operations are fail-open and fall back to the database path.
- Payment webhook idempotency is fail-safe: duplicate completed webhooks return success, in-progress duplicates return conflict, and unavailable idempotency store returns service unavailable.
- Lock failures return controlled conflict responses while database constraints remain the source of truth.

## Key Conventions

- `courses:list:{hash}` for course list queries.
- `courses:detail:{courseId}` for course detail.
- `analytics:dashboard:{scope}:{id}` and `analytics:{entity}:{id}` for reporting.
- `payment:webhook:{provider}:{transactionId}` for payment webhook idempotency.
- `lock:checkout:{buyerUserId}`, `lock:class-seat:{classId}`, `lock:coupon:{couponCode}`, `lock:license-pool:{poolId}` for distributed locks.

## Verification

- `dotnet test src/ELearning.sln --no-restore -m:1 /nr:false` passed.
- Frontend was not changed in this sprint, so `npm run build` was not required.

## Deferred

- Permission cache and AI rate/cost cache remain follow-up targets.
- Redis cluster/topology hardening remains infrastructure work for later performance/security sprints.
