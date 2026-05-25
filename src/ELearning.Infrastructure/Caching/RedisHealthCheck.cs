using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ELearning.Infrastructure.Caching;

public sealed class RedisHealthCheck(IRedisConnectionProvider redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null)
                return HealthCheckResult.Unhealthy("Redis is unavailable.");

            var pong = await db.PingAsync();
            return pong >= TimeSpan.Zero
                ? HealthCheckResult.Healthy("Redis is reachable.")
                : HealthCheckResult.Unhealthy("Redis ping failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis health check failed.", ex);
        }
    }
}
