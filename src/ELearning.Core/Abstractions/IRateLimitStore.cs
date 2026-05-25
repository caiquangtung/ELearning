namespace ELearning.Core.Abstractions;

public interface IRateLimitStore
{
    Task<RateLimitResult> IncrementAsync(string key, int limit, TimeSpan window, CancellationToken ct = default);
}

public sealed record RateLimitResult(
    bool IsAllowed,
    int Count,
    int Limit,
    DateTimeOffset ResetAt,
    bool IsStoreAvailable = true,
    string? FailureReason = null);
