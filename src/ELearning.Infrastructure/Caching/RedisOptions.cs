namespace ELearning.Infrastructure.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string? ConnectionString { get; init; }
    public int DefaultCacheTtlSeconds { get; init; } = 300;
    public int CourseDetailTtlSeconds { get; init; } = 600;
    public int AnalyticsTtlSeconds { get; init; } = 180;
    public int LockTtlSeconds { get; init; } = 10;
    public int IdempotencyTtlSeconds { get; init; } = 86_400;
    public int RateLimitWindowSeconds { get; init; } = 60;
}
