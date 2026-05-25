using ELearning.Core.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ELearning.Infrastructure.Caching;

public sealed class RedisDistributedLockService(
    IRedisConnectionProvider redis,
    ILogger<RedisDistributedLockService> logger)
    : IDistributedLockService
{
    public async Task<IDistributedLockHandle> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        if (db is null)
        {
            logger.LogWarning("Redis lock unavailable for key {LockKey}.", key);
            return RedisDistributedLockHandle.NotAcquired(key, "Redis is unavailable.");
        }

        var token = Guid.NewGuid().ToString("N");
        var acquired = await db.StringSetAsync(key, token, ttl, When.NotExists);
        if (!acquired)
        {
            logger.LogWarning("Redis lock acquisition failed for key {LockKey}.", key);
            return RedisDistributedLockHandle.NotAcquired(key, "Resource is locked.");
        }

        logger.LogInformation("Redis lock acquired for key {LockKey}.", key);
        return new RedisDistributedLockHandle(key, token, db, logger);
    }

    private sealed class RedisDistributedLockHandle(
        string key,
        string token,
        IDatabase? db,
        ILogger logger)
        : IDistributedLockHandle
    {
        private const string ReleaseScript =
            "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";

        public string Key { get; } = key;
        public bool Acquired { get; init; } = db is not null;
        public string? FailureReason { get; init; }

        public static RedisDistributedLockHandle NotAcquired(string key, string reason)
            => new(key, string.Empty, null, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance)
            {
                Acquired = false,
                FailureReason = reason
            };

        public async ValueTask DisposeAsync()
        {
            if (!Acquired || db is null) return;

            try
            {
                await db.ScriptEvaluateAsync(ReleaseScript, [Key], [token]);
                logger.LogInformation("Redis lock released for key {LockKey}.", Key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis lock release failed for key {LockKey}.", Key);
            }
        }
    }
}
