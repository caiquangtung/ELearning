using ELearning.Domain.Aggregates.NotificationAggregate;

namespace ELearning.Application.Features.Notifications.Common;

public static class NotificationMapper
{
    public static NotificationDto ToDto(Notification notification) =>
        new(
            notification.Id,
            notification.UserId,
            notification.MessageId,
            notification.Title,
            notification.Body,
            notification.Type.ToString(),
            notification.ActionUrl,
            notification.IsRead,
            notification.ReadAt,
            notification.CreatedAt);

    public static MessageDto ToDto(Message message) =>
        new(
            message.Id,
            message.SenderUserId,
            message.Subject,
            message.Body,
            message.Scope.ToString(),
            message.OrganizationId,
            message.CourseId,
            message.TrainingClassId,
            message.RecipientCount,
            message.SentAt);
}
