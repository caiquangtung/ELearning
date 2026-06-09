namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeReindexQueue
{
    Task<Guid> EnqueueAsync(Guid? courseId, CancellationToken ct = default);
}
