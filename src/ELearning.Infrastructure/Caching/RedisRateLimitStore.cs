using ELearning.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Caching;

public sealed class RedisRateLimitStore(
    IRedisConnectionProvider redis,
    ILogger<RedisRateLimitStore> logger)
    : IRateLimitStore
{
    public async Task<RateLimitResult> IncrementAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
    {
        var resetAt = DateTimeOffset.UtcNow.Add(window);
        try
        {
            var db = redis.GetDatabase();
            if (db is null)
                return new RateLimitResult(true, 0, limit, resetAt, false, "Redis is unavailable.");

            var count = await db.StringIncrementAsync(key);
            if (count == 1)
                await db.KeyExpireAsync(key, window);

            var allowed = count <= limit;
            if (!allowed)
                logger.LogWarning("Rate limit exceeded for key {RateLimitKey}. Count {Count}, limit {Limit}.", key, count, limit);

            return new RateLimitResult(allowed, (int)count, limit, resetAt);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rate limit store failed for key {RateLimitKey}.", key);
            return new RateLimitResult(true, 0, limit, resetAt, false, ex.Message);
        }
    }
}
