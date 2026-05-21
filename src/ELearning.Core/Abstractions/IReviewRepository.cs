using ELearning.Core.Common;
using ELearning.Domain.Aggregates.ReviewAggregate;

namespace ELearning.Core.Abstractions;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetForCourseAndUserAsync(Guid courseId, Guid userId, CancellationToken ct = default);
    Task<PagedList<Review>> ListForCourseAsync(Guid courseId, int page, int pageSize, bool includeRejected, CancellationToken ct = default);
    Task<(decimal AverageRating, int ReviewCount)> GetSummaryForCourseAsync(Guid courseId, CancellationToken ct = default);
}
