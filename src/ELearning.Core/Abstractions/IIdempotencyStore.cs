namespace ELearning.Core.Abstractions;

public interface IIdempotencyStore
{
    Task<IdempotencyBeginResult> TryBeginAsync(string key, TimeSpan ttl, CancellationToken ct = default);
    Task CompleteAsync(string key, TimeSpan ttl, CancellationToken ct = default);
    Task FailAsync(string key, TimeSpan ttl, CancellationToken ct = default);
}

public enum IdempotencyBeginStatus
{
    Started,
    InProgress,
    Completed,
    Unavailable
}

public sealed record IdempotencyBeginResult(IdempotencyBeginStatus Status, string? FailureReason = null)
{
    public bool Started => Status == IdempotencyBeginStatus.Started;
}
