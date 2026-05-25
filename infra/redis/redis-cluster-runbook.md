# Redis Cluster Runbook

## Topology

- Production: 3 masters + 3 replicas, spread across at least 3 availability zones when the platform supports it.
- Staging: 3 masters without replicas is acceptable only for cost-controlled smoke testing.
- Development: single Redis instance remains supported by `Redis:ConnectionString=localhost:6379`.

## Workload Separation

Use separate Redis deployments when possible:

- `cache`: catalog/dashboard analytics cache; eviction policy can be `allkeys-lru`.
- `consistency`: distributed locks, idempotency, and rate-limit counters; eviction policy should be `noeviction`.

The application key prefixes already separate workloads (`courses:*`, `analytics:*`, `lock:*`, `payment:webhook:*`, `rate:*`), but prefix separation is not a replacement for production memory isolation.

## Required Settings

- Enable append-only persistence for the consistency deployment.
- Set memory alerts at 70%, 80%, and 90%.
- Set connection alerts for sustained reconnects or timeout spikes.
- Keep Redis server time synchronized with NTP because lock and idempotency TTLs depend on consistent expiry behavior.

## Application Configuration

Use a clustered endpoint or managed Redis primary endpoint:

```json
{
  "Redis": {
    "ConnectionString": "redis-cluster.example.internal:6379",
    "DefaultCacheTtlSeconds": 300,
    "CourseDetailTtlSeconds": 600,
    "LockTtlSeconds": 30,
    "IdempotencyTtlSeconds": 86400,
    "RateLimitWindowSeconds": 60
  }
}
```

For managed Redis with TLS, include the provider-specific TLS option in the connection string.
