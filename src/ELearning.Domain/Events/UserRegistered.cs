using ELearning.Domain.Shared;

namespace ELearning.Domain.Events;

public sealed record UserRegistered(
    Guid UserId,
    string Email,
    string FullName) : DomainEvent;
