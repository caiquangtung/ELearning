using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;

namespace ELearning.Core.Abstractions;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<Quiz?> GetByIdWithQuestionsAsync(Guid id, CancellationToken ct = default);
    Task<PagedList<Quiz>> ListAsync(
        int page,
        int pageSize,
        string? search,
        QuizStatus? status,
        CancellationToken ct = default);
}
