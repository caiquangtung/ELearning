using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendAnnouncement;

public sealed class SendAnnouncementCommandHandler(
    IMessageRepository messageRepository,
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendAnnouncementCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendAnnouncementCommand request, CancellationToken ct)
    {
        var senderUserId = currentUserService.UserId;
        if (!senderUserId.HasValue)
            return Result.Failure<MessageDto>(Error.Unauthorized());

        var recipients = request.RecipientUserIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (recipients.Count == 0)
            return Result.Failure<MessageDto>(Error.Validation("Recipients", "At least one recipient is required."));

        foreach (var recipientId in recipients)
        {
            if (!await userRepository.ExistsAsync(u => u.Id == recipientId, ct))
                return Result.Failure<MessageDto>(Error.NotFound("User", recipientId));
        }

        try
        {
            var message = Message.CreateAnnouncement(
                senderUserId.Value,
                request.Subject,
                request.Body,
                request.Scope,
                recipients.Count,
                request.OrganizationId,
                request.CourseId,
                request.TrainingClassId);

            messageRepository.Add(message);

            foreach (var recipientId in recipients)
            {
                notificationRepository.Add(Notification.Create(
                    recipientId,
                    request.Subject,
                    request.Body,
                    NotificationType.Announcement,
                    request.ActionUrl,
                    message.Id));
            }

            await unitOfWork.SaveChangesAsync(ct);

            return NotificationMapper.ToDto(message);
        }
        catch (DomainException ex)
        {
            return Result.Failure<MessageDto>(Error.Validation("Message", ex.Message));
        }
    }
}
