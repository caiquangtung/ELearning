using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarkNotificationReadCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<NotificationDto>(Error.Unauthorized());

        var notification = await notificationRepository.GetForUserAsync(request.NotificationId, userId.Value, ct);
        if (notification is null)
            return Result.Failure<NotificationDto>(Error.NotFound("Notification", request.NotificationId));

        notification.MarkAsRead(DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);

        return NotificationMapper.ToDto(notification);
    }
}
