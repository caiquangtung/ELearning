using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Quizzes;

public class QuizAttemptRepository(ApplicationDbContext context) : IQuizAttemptRepository
{
    private readonly DbSet<QuizAttempt> _dbSet = context.Set<QuizAttempt>();

    public async Task<QuizAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<QuizAttempt?> GetByIdWithAnswersAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<QuizAttempt?> GetInProgressAsync(Guid quizId, Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                a => a.QuizId == quizId && a.UserId == userId && a.Status == AttemptStatus.InProgress,
                ct);
    }

    public async Task<IReadOnlyList<QuizAttempt>> ListByQuizAsync(Guid quizId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.QuizId == quizId)
            .ToListAsync(ct);
    }

    public void Add(QuizAttempt attempt) => _dbSet.Add(attempt);
    public void Update(QuizAttempt attempt) => _dbSet.Update(attempt);
}
