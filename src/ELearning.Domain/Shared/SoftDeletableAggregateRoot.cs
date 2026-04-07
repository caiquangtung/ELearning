namespace ELearning.Domain.Shared;

public abstract class SoftDeletableAggregateRoot : AuditableAggregateRoot
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
}

