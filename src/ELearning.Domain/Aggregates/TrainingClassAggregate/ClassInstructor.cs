using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.TrainingClassAggregate;

public sealed class ClassInstructor : Entity
{
    private ClassInstructor() { }

    public Guid TrainingClassId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    internal static ClassInstructor Create(Guid trainingClassId, Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            TrainingClassId = trainingClassId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        };
}
