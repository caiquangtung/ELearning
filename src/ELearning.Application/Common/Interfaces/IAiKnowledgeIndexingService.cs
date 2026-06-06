namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeIndexingService
{
    Task<AiKnowledgeReindexResult> ReindexAsync(Guid? courseId, CancellationToken ct = default);
}

public sealed record AiKnowledgeReindexResult(
    int IndexedCourses,
    int IndexedChunks,
    int DeletedStaleChunks);
