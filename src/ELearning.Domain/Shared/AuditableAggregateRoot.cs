namespace ELearning.Domain.Shared;

public abstract class AuditableAggregateRoot : AggregateRoot
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
}

