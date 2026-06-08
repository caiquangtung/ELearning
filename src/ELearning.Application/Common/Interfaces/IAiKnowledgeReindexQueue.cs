namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeReindexQueue
{
    ValueTask EnqueueAsync(Guid? courseId, CancellationToken ct = default);
}
