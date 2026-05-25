using ELearning.Core.Abstractions;
using ELearning.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ELearning.Application.UnitTests;

public class RedisInfrastructureTests
{
    [Fact]
    public void CacheKeyBuilder_creates_stable_hash_keys()
    {
        var builder = new CacheKeyBuilder();
        var request = new { Page = 1, PageSize = 20, Search = "dotnet" };

        var first = builder.BuildHashKey("courses:list", request);
        var second = builder.BuildHashKey("courses:list", request);

        first.Should().Be(second);
        first.Should().StartWith("courses:list:");
    }

    [Fact]
    public async Task Cache_get_or_create_falls_back_to_factory_when_redis_is_unavailable()
    {
        var cache = new RedisCacheService(
            new UnavailableRedisProvider(),
            Options.Create(new RedisOptions()),
            NullLogger<RedisCacheService>.Instance);

        var value = await cache.GetOrCreateAsync("courses:list:test", _ => Task.FromResult("from-db"));

        value.Should().Be("from-db");
    }

    [Fact]
    public async Task Idempotency_store_reports_unavailable_when_redis_is_down()
    {
        var store = new RedisIdempotencyStore(
            new UnavailableRedisProvider(),
            NullLogger<RedisIdempotencyStore>.Instance);

        var result = await store.TryBeginAsync("payment:webhook:noop:tx", TimeSpan.FromMinutes(5));

        result.Status.Should().Be(IdempotencyBeginStatus.Unavailable);
    }

    [Fact]
    public async Task Distributed_lock_reports_not_acquired_when_redis_is_down()
    {
        var locks = new RedisDistributedLockService(
            new UnavailableRedisProvider(),
            NullLogger<RedisDistributedLockService>.Instance);

        await using var handle = await locks.AcquireAsync("lock:checkout:user", TimeSpan.FromSeconds(10));

        handle.Acquired.Should().BeFalse();
        handle.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Rate_limit_store_fails_open_when_redis_is_down()
    {
        var store = new RedisRateLimitStore(
            new UnavailableRedisProvider(),
            NullLogger<RedisRateLimitStore>.Instance);

        var result = await store.IncrementAsync("rate:auth:user", 5, TimeSpan.FromMinutes(1));

        result.IsAllowed.Should().BeTrue();
        result.IsStoreAvailable.Should().BeFalse();
    }

    private sealed class UnavailableRedisProvider : IRedisConnectionProvider
    {
        public IDatabase? GetDatabase() => null;
        public IServer? GetServer() => null;
    }
}
