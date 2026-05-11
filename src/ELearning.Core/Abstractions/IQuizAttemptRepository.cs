using ELearning.Domain.Aggregates.QuizAggregate;

namespace ELearning.Core.Abstractions;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<QuizAttempt?> GetByIdWithAnswersAsync(Guid id, CancellationToken ct = default);
    Task<QuizAttempt?> GetInProgressAsync(Guid quizId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<QuizAttempt>> ListByQuizAsync(Guid quizId, CancellationToken ct = default);
    void Add(QuizAttempt attempt);
    void Update(QuizAttempt attempt);
}
