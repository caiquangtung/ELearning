using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiChatSession : AuditableAggregateRoot
{
    private AiChatSession() { }

    public Guid UserId { get; private set; }
    public Guid? CourseId { get; private set; }
    public string Title { get; private set; } = default!;
    public List<AiChatMessage> Messages { get; private set; } = [];

    public static AiChatSession Create(Guid userId, Guid? courseId, string title)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Chat session user is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Chat session title is required.");

        return new AiChatSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            Title = title.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
