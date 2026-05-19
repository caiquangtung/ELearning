namespace ELearning.Application.Features.Notifications.Common;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    Guid? MessageId,
    string Title,
    string Body,
    string Type,
    string? ActionUrl,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);

public sealed record MessageDto(
    Guid Id,
    Guid SenderUserId,
    string Subject,
    string Body,
    string Scope,
    Guid? OrganizationId,
    Guid? CourseId,
    Guid? TrainingClassId,
    int RecipientCount,
    DateTime SentAt);

public sealed record UnreadNotificationCountDto(int Count);
