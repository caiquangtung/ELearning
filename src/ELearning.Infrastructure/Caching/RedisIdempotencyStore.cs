using ELearning.Core.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ELearning.Infrastructure.Caching;

public sealed class RedisIdempotencyStore(
    IRedisConnectionProvider redis,
    ILogger<RedisIdempotencyStore> logger)
    : IIdempotencyStore
{
    private const string InProgress = "in-progress";
    private const string Completed = "completed";
    private const string Failed = "failed";

    public async Task<IdempotencyBeginResult> TryBeginAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null)
                return new IdempotencyBeginResult(IdempotencyBeginStatus.Unavailable, "Redis is unavailable.");

            var started = await db.StringSetAsync(key, InProgress, ttl, When.NotExists);
            if (started)
            {
                logger.LogInformation("Idempotency key started: {IdempotencyKey}.", key);
                return new IdempotencyBeginResult(IdempotencyBeginStatus.Started);
            }

            var existing = await db.StringGetAsync(key);
            var status = existing.ToString() == Completed
                ? IdempotencyBeginStatus.Completed
                : IdempotencyBeginStatus.InProgress;
            logger.LogWarning("Idempotency duplicate for key {IdempotencyKey} with status {Status}.", key, status);
            return new IdempotencyBeginResult(status);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Idempotency begin failed for key {IdempotencyKey}.", key);
            return new IdempotencyBeginResult(IdempotencyBeginStatus.Unavailable, ex.Message);
        }
    }

    public async Task CompleteAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        => await SetStatusAsync(key, Completed, ttl);

    public async Task FailAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        => await SetStatusAsync(key, Failed, ttl);

    private async Task SetStatusAsync(string key, string status, TimeSpan ttl)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null) return;

            await db.StringSetAsync(key, status, ttl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Idempotency status update failed for key {IdempotencyKey}.", key);
        }
    }
}
