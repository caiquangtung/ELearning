using ELearning.Domain.Aggregates.VideoAggregate;

namespace ELearning.Core.Abstractions;

public interface IVideoAssetRepository : IRepository<VideoAsset>
{
    Task<VideoAsset?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task<IReadOnlyList<VideoAsset>> ListByLessonIdsAsync(IReadOnlyCollection<Guid> lessonIds, CancellationToken ct = default);
}
