using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Videos;

public sealed class WatchEventRepository(ApplicationDbContext context)
    : GenericRepository<WatchEvent>(context), IWatchEventRepository
{
    public async Task<WatchEvent?> GetForUserAsync(Guid videoAssetId, Guid userId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(w => w.VideoAssetId == videoAssetId && w.UserId == userId, ct);

    public async Task<int> CountCompletedForVideosAsync(
        IReadOnlyCollection<Guid> videoAssetIds,
        Guid userId,
        CancellationToken ct = default)
    {
        if (videoAssetIds.Count == 0)
            return 0;

        return await DbSet.AsNoTracking().CountAsync(
            w => videoAssetIds.Contains(w.VideoAssetId) && w.UserId == userId && w.IsCompleted,
            ct);
    }
}
