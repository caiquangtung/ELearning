namespace ELearning.Core.Abstractions;

public interface IDistributedLockService
{
    Task<IDistributedLockHandle> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct = default);
}

public interface IDistributedLockHandle : IAsyncDisposable
{
    string Key { get; }
    bool Acquired { get; }
    string? FailureReason { get; }
}
