using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.GetUnreadCount;

public sealed record GetUnreadNotificationCountQuery : IRequest<Result<UnreadNotificationCountDto>>;
