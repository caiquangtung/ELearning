using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Reviews;

public sealed class ReviewRepository(ApplicationDbContext context)
    : GenericRepository<Review>(context), IReviewRepository
{
    public async Task<Review?> GetForCourseAndUserAsync(Guid courseId, Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId, ct);

    public async Task<PagedList<Review>> ListForCourseAsync(
        Guid courseId,
        int page,
        int pageSize,
        bool includeRejected,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

        var query = DbSet.AsNoTracking().Where(r => r.CourseId == courseId);
        if (!includeRejected)
            query = query.Where(r => r.Status == ReviewStatus.Published);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Review>.Create(items, page, pageSize, total);
    }

    public async Task<(decimal AverageRating, int ReviewCount)> GetSummaryForCourseAsync(Guid courseId, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(r => r.CourseId == courseId && r.Status == ReviewStatus.Published);

        var count = await query.CountAsync(ct);
        if (count == 0)
            return (0m, 0);

        var average = await query.AverageAsync(r => r.Rating, ct);
        return (Math.Round((decimal)average, 2), count);
    }
}
