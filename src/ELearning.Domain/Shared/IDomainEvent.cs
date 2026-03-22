using MediatR;

namespace ELearning.Domain.Shared;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
