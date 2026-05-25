# Sprint 15 Infrastructure Runbook

## CDN

- Put the Angular static host behind CDN.
- Cache hashed assets (`*.js`, `*.css`, fonts, images) for 30 days or longer with immutable cache headers.
- Keep `index.html`, `manifest.webmanifest`, and `sw.js` on `no-cache` or very short TTL so deployments can roll forward quickly.
- Do not cache `/api/*` at CDN unless a specific endpoint has explicit `Cache-Control` and no user-specific data.

## Load Balancer

- Use least-connections or equivalent algorithm for API nodes.
- Forward `Host`, `X-Real-IP`, `X-Forwarded-For`, and `X-Forwarded-Proto`.
- Use short connect timeout and longer read timeout for upload/payment workflows.
- Keep `/api/*` uncached at the load balancer. Static assets can use the cache behavior in `infra/nginx/elearning-load-balancer.conf`.

## Redis

- Use the Redis cluster guidance in `infra/redis/redis-cluster-runbook.md`.
- Prefer separate deployments for cache and consistency primitives.
- Treat Redis cache failures as fail-open; treat lock/idempotency failures for payment/checkout/license/coupon as fail-safe.

## Rollout Checks

- Verify `/health` includes Redis health in staging.
- Run duplicate payment webhook smoke test before production release.
- Run checkout and license assignment concurrency smoke tests with at least two API nodes.
- Verify service worker registration only in production builds and that `/api/*` responses are not cached.
