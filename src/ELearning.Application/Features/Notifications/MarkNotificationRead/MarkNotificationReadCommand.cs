using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId)
    : IRequest<Result<NotificationDto>>;
