using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Videos;

public sealed class VideoAssetRepository(ApplicationDbContext context)
    : GenericRepository<VideoAsset>(context), IVideoAssetRepository
{
    public async Task<VideoAsset?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(v => v.LessonId == lessonId, ct);

    public async Task<IReadOnlyList<VideoAsset>> ListByLessonIdsAsync(
        IReadOnlyCollection<Guid> lessonIds,
        CancellationToken ct = default)
    {
        if (lessonIds.Count == 0)
            return [];

        return await DbSet.AsNoTracking()
            .Where(v => lessonIds.Contains(v.LessonId))
            .ToListAsync(ct);
    }
}
