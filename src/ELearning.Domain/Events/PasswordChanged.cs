using ELearning.Domain.Shared;

namespace ELearning.Domain.Events;

public sealed record PasswordChanged(Guid UserId, string Email) : DomainEvent;
