using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendEmail;

public sealed record SendEmailCommand(string To, string Subject, string Body)
    : IRequest<Result>;
