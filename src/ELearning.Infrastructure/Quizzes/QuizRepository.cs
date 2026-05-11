using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Quizzes;

public class QuizRepository(ApplicationDbContext context)
    : GenericRepository<Quiz>(context), IQuizRepository
{
    public async Task<Quiz?> GetByIdWithQuestionsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .AsSplitQuery()
            .Include(q => q.Questions)
            .ThenInclude(qn => qn.Options)
            .FirstOrDefaultAsync(q => q.Id == id, ct);
    }

    public async Task<PagedList<Quiz>> ListAsync(
        int page,
        int pageSize,
        string? search,
        QuizStatus? status,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(q => q.Title.ToLower().Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Quiz>.Create(items, page, pageSize, totalCount);
    }
}
