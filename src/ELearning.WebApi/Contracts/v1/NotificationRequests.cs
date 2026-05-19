using ELearning.Domain.Aggregates.NotificationAggregate;

namespace ELearning.WebApi.Contracts.v1;

public sealed record ListNotificationsRequest(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false);

public sealed record SendNotificationRequest(
    Guid UserId,
    string Title,
    string Body,
    NotificationType Type = NotificationType.Info,
    string? ActionUrl = null);

public sealed record SendAnnouncementRequest(
    IReadOnlyCollection<Guid> RecipientUserIds,
    string Subject,
    string Body,
    MessageScope Scope = MessageScope.Platform,
    Guid? OrganizationId = null,
    Guid? CourseId = null,
    Guid? TrainingClassId = null,
    string? ActionUrl = null);

public sealed record SendEmailRequest(string To, string Subject, string Body);
