using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendNotification;

public sealed class SendNotificationCommandHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendNotificationCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand request, CancellationToken ct)
    {
        if (!await userRepository.ExistsAsync(u => u.Id == request.UserId, ct))
            return Result.Failure<NotificationDto>(Error.NotFound("User", request.UserId));

        try
        {
            var notification = Notification.Create(
                request.UserId,
                request.Title,
                request.Body,
                request.Type,
                request.ActionUrl);

            notificationRepository.Add(notification);
            await unitOfWork.SaveChangesAsync(ct);

            return NotificationMapper.ToDto(notification);
        }
        catch (DomainException ex)
        {
            return Result.Failure<NotificationDto>(Error.Validation("Notification", ex.Message));
        }
    }
}
