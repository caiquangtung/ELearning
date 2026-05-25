using System.Text.Json;
using ELearning.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ELearning.Infrastructure.Caching;

public sealed class RedisCacheService(
    IRedisConnectionProvider redis,
    IOptions<RedisOptions> options,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null)
            {
                logger.LogWarning("Cache unavailable for key {CacheKey}.", key);
                return default;
            }

            var value = await db.StringGetAsync(key);
            if (!value.HasValue)
            {
                logger.LogInformation("Cache miss for key {CacheKey}.", key);
                return default;
            }

            logger.LogInformation("Cache hit for key {CacheKey}.", key);
            return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache get failed for key {CacheKey}.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null) return;

            var json = JsonSerializer.Serialize(value, JsonOptions);
            await db.StringSetAsync(key, json, ttl ?? TimeSpan.FromSeconds(options.Value.DefaultCacheTtlSeconds));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache set failed for key {CacheKey}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            if (db is null) return;

            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache remove failed for key {CacheKey}.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var server = redis.GetServer();
            if (db is null || server is null) return;

            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Length > 0)
                await db.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache prefix remove failed for prefix {CachePrefix}.", prefix);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null)
            return cached;

        var value = await factory(ct);
        await SetAsync(key, value, ttl, ct);
        return value;
    }
}
