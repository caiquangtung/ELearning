using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendNotification;

public sealed record SendNotificationCommand(
    Guid UserId,
    string Title,
    string Body,
    NotificationType Type = NotificationType.Info,
    string? ActionUrl = null)
    : IRequest<Result<NotificationDto>>;
