using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.ListMyNotifications;

public sealed record ListMyNotificationsQuery(int Page = 1, int PageSize = 20, bool UnreadOnly = false)
    : IRequest<Result<PagedList<NotificationDto>>>;
