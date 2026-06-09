using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiKnowledgeReindexJob : AuditableAggregateRoot
{
    private AiKnowledgeReindexJob() { }

    public Guid? CourseId { get; private set; }
    public AiKnowledgeReindexJobStatus Status { get; private set; }
    public Guid? RequestedByUserId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int IndexedCourses { get; private set; }
    public int IndexedChunks { get; private set; }
    public int DeletedStaleChunks { get; private set; }
    public string? Error { get; private set; }

    public static AiKnowledgeReindexJob Create(Guid? courseId, Guid? requestedByUserId) =>
        new()
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            RequestedByUserId = requestedByUserId,
            Status = AiKnowledgeReindexJobStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };

    public void MarkInProgress()
    {
        Status = AiKnowledgeReindexJobStatus.InProgress;
        StartedAt ??= DateTime.UtcNow;
        CompletedAt = null;
        Error = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded(int indexedCourses, int indexedChunks, int deletedStaleChunks)
    {
        Status = AiKnowledgeReindexJobStatus.Succeeded;
        IndexedCourses = indexedCourses;
        IndexedChunks = indexedChunks;
        DeletedStaleChunks = deletedStaleChunks;
        CompletedAt = DateTime.UtcNow;
        Error = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = AiKnowledgeReindexJobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        Error = string.IsNullOrWhiteSpace(error) ? "Reindex failed." : error.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
