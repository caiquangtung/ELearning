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
        long? minPriceCents,
        long? maxPriceCents,
        CourseSortOption sort,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var q = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(c =>
                c.Title.ToLower().Contains(s) ||
                (c.Description != null && c.Description.ToLower().Contains(s)) ||
                c.Sections.Any(section => section.Lessons.Any(lesson =>
                    lesson.Title.ToLower().Contains(s) ||
                    (lesson.Content != null && lesson.Content.ToLower().Contains(s)))));
        }

        if (status.HasValue)
            q = q.Where(c => c.Status == status.Value);

        if (minPriceCents.HasValue)
            q = q.Where(c => c.PriceCents >= minPriceCents.Value);

        if (maxPriceCents.HasValue)
            q = q.Where(c => c.PriceCents <= maxPriceCents.Value);

        q = sort switch
        {
            CourseSortOption.Oldest => q.OrderBy(c => c.CreatedAt),
            CourseSortOption.TitleAsc => q.OrderBy(c => c.Title),
            CourseSortOption.TitleDesc => q.OrderByDescending(c => c.Title),
            CourseSortOption.PriceAsc => q.OrderBy(c => c.PriceCents).ThenByDescending(c => c.CreatedAt),
            CourseSortOption.PriceDesc => q.OrderByDescending(c => c.PriceCents).ThenByDescending(c => c.CreatedAt),
            _ => q.OrderByDescending(c => c.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Course>.Create(items, page, pageSize, total);
    }
}
