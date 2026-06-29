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
    int QueuedJobs,
    int InProgressJobs,
    int FailedJobs,
    int FailedAiRequests,
    int VectorDimensions,
    string VectorProvider,
    string VectorModel,
    AiKnowledgeReindexJobDto? LastJob,
    IReadOnlyList<AiKnowledgeReindexJobDto> RecentJobs,
    AiRagEvaluationRunDto? LastEvaluation,
    IReadOnlyList<AiRagEvaluationRunDto> RecentEvaluations);

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

public sealed record AiRagEvaluationRunDto(
    Guid Id,
    string Status,
    Guid? RequestedByUserId,
    string DatasetVersion,
    int TotalCases,
    int PassedCases,
    decimal RetrievalHitRate,
    decimal CitationValidityRate,
    decimal RefusalAccuracyRate,
    decimal GroundednessRate,
    string? Error,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);
