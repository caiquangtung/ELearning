using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.GetUnreadCount;

public sealed class GetUnreadNotificationCountQueryHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<UnreadNotificationCountDto>>
{
    public async Task<Result<UnreadNotificationCountDto>> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<UnreadNotificationCountDto>(Error.Unauthorized());

        var count = await notificationRepository.CountUnreadAsync(userId.Value, ct);
        return new UnreadNotificationCountDto(count);
    }
}
