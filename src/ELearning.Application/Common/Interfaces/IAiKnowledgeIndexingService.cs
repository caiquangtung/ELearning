namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeIndexingService
{
    Task<AiKnowledgeReindexResult> ReindexAsync(
        Guid? courseId,
        Guid? requestedByUserId = null,
        Guid? jobId = null,
        CancellationToken ct = default);

    Task<AiKnowledgeStatusResult> GetStatusAsync(CancellationToken ct = default);
}

public sealed record AiKnowledgeReindexResult(
    Guid JobId,
    int IndexedCourses,
    int IndexedChunks,
    int DeletedStaleChunks);

public sealed record AiKnowledgeStatusResult(
    int TotalChunks,
    int VectorizedChunks,
    int IndexedCourses,
    int FailedJobs,
    int VectorDimensions,
    string VectorProvider,
    string VectorModel,
    AiKnowledgeReindexJobSummary? LastJob,
    IReadOnlyList<AiKnowledgeReindexJobSummary> RecentJobs);

public sealed record AiKnowledgeReindexJobSummary(
    Guid Id,
    Guid? CourseId,
    string Status,
    Guid? RequestedByUserId,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int IndexedCourses,
    int IndexedChunks,
    int DeletedStaleChunks,
    string? Error,
    DateTime CreatedAt);
