using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.TrainingClasses;

public sealed class TrainingClassRepository(ApplicationDbContext context)
    : GenericRepository<TrainingClass>(context), ITrainingClassRepository
{
    public async Task<TrainingClass?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(t => t.Instructors)
            .Include(t => t.Sessions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedList<TrainingClass>> ListAsync(
        int page,
        int pageSize,
        Guid? courseId,
        string? search,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var q = DbSet.AsNoTracking().AsQueryable();

        if (courseId.HasValue)
            q = q.Where(t => t.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(t => t.Title.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<TrainingClass>.Create(items, page, pageSize, total);
    }

    public async Task<bool> HasInstructorSessionOverlapAsync(
        Guid userId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeSessionId,
        CancellationToken ct = default)
    {
        var sessions = DbContext.Set<ClassSession>();
        var instructors = DbContext.Set<ClassInstructor>();
        var classes = DbContext.Set<TrainingClass>();

        var query =
            from s in sessions
            join tc in classes on s.TrainingClassId equals tc.Id
            join ci in instructors on tc.Id equals ci.TrainingClassId
            where ci.UserId == userId
                  && s.Status != ClassSessionStatus.Cancelled
                  && (excludeSessionId == null || s.Id != excludeSessionId.Value)
                  && s.StartUtc < endUtc
                  && s.EndUtc > startUtc
            select s.Id;

        return await query.AnyAsync(ct);
    }
}
