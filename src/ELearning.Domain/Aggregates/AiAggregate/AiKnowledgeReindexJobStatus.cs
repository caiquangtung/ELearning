namespace ELearning.Domain.Aggregates.AiAggregate;

public enum AiKnowledgeReindexJobStatus
{
    Queued = 0,
    InProgress = 1,
    Succeeded = 2,
    Failed = 3
}
