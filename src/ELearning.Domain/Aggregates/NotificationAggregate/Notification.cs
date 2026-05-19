using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.NotificationAggregate;

public sealed class Notification : AuditableAggregateRoot
{
    private Notification() { }

    public Guid UserId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public NotificationType Type { get; private set; }
    public string? ActionUrl { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public bool IsRead => ReadAt.HasValue;

    public static Notification Create(
        Guid userId,
        string title,
        string body,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        Guid? messageId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Notification recipient is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Notification title is required.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Notification body is required.");

        var now = DateTime.UtcNow;
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MessageId = messageId,
            Title = title.Trim(),
            Body = body.Trim(),
            Type = type,
            ActionUrl = string.IsNullOrWhiteSpace(actionUrl) ? null : actionUrl.Trim(),
            CreatedAt = now
        };
    }

    public void MarkAsRead(DateTime utcNow)
    {
        if (ReadAt.HasValue)
            return;

        ReadAt = utcNow;
        UpdatedAt = utcNow;
    }
}
