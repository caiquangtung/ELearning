using ELearning.Domain.Aggregates.VideoAggregate;

namespace ELearning.Core.Abstractions;

public interface IWatchEventRepository : IRepository<WatchEvent>
{
    Task<WatchEvent?> GetForUserAsync(Guid videoAssetId, Guid userId, CancellationToken ct = default);
    Task<int> CountCompletedForVideosAsync(IReadOnlyCollection<Guid> videoAssetIds, Guid userId, CancellationToken ct = default);
}
