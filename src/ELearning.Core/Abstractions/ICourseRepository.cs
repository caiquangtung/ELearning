using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;

namespace ELearning.Core.Abstractions;

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<PagedList<Course>> ListAsync(
        int page,
        int pageSize,
        string? search,
        CourseStatus? status,
        CancellationToken ct = default);
}

