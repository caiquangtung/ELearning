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
}
