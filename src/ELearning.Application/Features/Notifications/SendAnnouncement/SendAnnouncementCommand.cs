using ELearning.Application.Features.Notifications.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendAnnouncement;

public sealed record SendAnnouncementCommand(
    IReadOnlyCollection<Guid> RecipientUserIds,
    string Subject,
    string Body,
    MessageScope Scope = MessageScope.Platform,
    Guid? OrganizationId = null,
    Guid? CourseId = null,
    Guid? TrainingClassId = null,
    string? ActionUrl = null)
    : IRequest<Result<MessageDto>>;
