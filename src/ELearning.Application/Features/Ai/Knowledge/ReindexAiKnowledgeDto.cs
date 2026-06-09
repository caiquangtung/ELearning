namespace ELearning.Application.Features.Ai.Knowledge;

public sealed record ReindexAiKnowledgeDto(
    Guid JobId,
    int IndexedCourses,
    int IndexedChunks,
    int DeletedStaleChunks);

public sealed record AiKnowledgeStatusDto(
    int TotalChunks,
    int VectorizedChunks,
    int IndexedCourses,
    int FailedJobs,
    int VectorDimensions,
    string VectorProvider,
    string VectorModel,
    AiKnowledgeReindexJobDto? LastJob,
    IReadOnlyList<AiKnowledgeReindexJobDto> RecentJobs);

public sealed record AiKnowledgeReindexJobDto(
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
