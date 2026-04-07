using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;

namespace ELearning.Core.Abstractions;

public interface ITrainingClassRepository : IRepository<TrainingClass>
{
    Task<TrainingClass?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<PagedList<TrainingClass>> ListAsync(
        int page,
        int pageSize,
        Guid? courseId,
        string? search,
        CancellationToken ct = default);

    /// <summary>
    /// True if the user is an instructor on some class and has another non-cancelled session
    /// overlapping [startUtc, endUtc).
    /// </summary>
    Task<bool> HasInstructorSessionOverlapAsync(
        Guid userId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeSessionId,
        CancellationToken ct = default);
}
