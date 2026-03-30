using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Courses;

public class CourseRepository(ApplicationDbContext context)
    : GenericRepository<Course>(context), ICourseRepository
{
    public async Task<Course?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                    .ThenInclude(l => l.Assets)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedList<Course>> ListAsync(
        int page,
        int pageSize,
        string? search,
        CourseStatus? status,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var q = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(c => c.Title.ToLower().Contains(s));
        }

        if (status.HasValue)
            q = q.Where(c => c.Status == status.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Course>.Create(items, page, pageSize, total);
    }
}

