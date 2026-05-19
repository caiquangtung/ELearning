using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.NotificationAggregate;

public sealed class Message : AuditableAggregateRoot
{
    private Message() { }

    public Guid SenderUserId { get; private set; }
    public string Subject { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public MessageScope Scope { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public Guid? CourseId { get; private set; }
    public Guid? TrainingClassId { get; private set; }
    public int RecipientCount { get; private set; }
    public DateTime SentAt { get; private set; }

    public static Message CreateAnnouncement(
        Guid senderUserId,
        string subject,
        string body,
        MessageScope scope,
        int recipientCount,
        Guid? organizationId = null,
        Guid? courseId = null,
        Guid? trainingClassId = null)
    {
        if (senderUserId == Guid.Empty)
            throw new DomainException("Message sender is required.");
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Message subject is required.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Message body is required.");
        if (recipientCount <= 0)
            throw new DomainException("At least one recipient is required.");

        var now = DateTime.UtcNow;
        return new Message
        {
            Id = Guid.NewGuid(),
            SenderUserId = senderUserId,
            Subject = subject.Trim(),
            Body = body.Trim(),
            Scope = scope,
            OrganizationId = organizationId,
            CourseId = courseId,
            TrainingClassId = trainingClassId,
            RecipientCount = recipientCount,
            SentAt = now,
            CreatedAt = now
        };
    }
}
