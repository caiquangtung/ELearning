using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.ListMyNotifications;

public sealed class ListMyNotificationsQueryHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListMyNotificationsQuery, Result<PagedList<NotificationDto>>>
{
    public async Task<Result<PagedList<NotificationDto>>> Handle(ListMyNotificationsQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<PagedList<NotificationDto>>(Error.Unauthorized());

        var paged = await notificationRepository.ListForUserAsync(
            userId.Value,
            request.Page,
            request.PageSize,
            request.UnreadOnly,
            ct);

        var dto = PagedList<NotificationDto>.Create(
            paged.Items.Select(NotificationMapper.ToDto).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return dto;
    }
}
