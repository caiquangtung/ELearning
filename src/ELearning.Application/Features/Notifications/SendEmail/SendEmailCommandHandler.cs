using ELearning.Application.Common.Interfaces;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Notifications.SendEmail;

public sealed class SendEmailCommandHandler(IEmailService emailService)
    : IRequestHandler<SendEmailCommand, Result>
{
    public async Task<Result> Handle(SendEmailCommand request, CancellationToken ct)
    {
        await emailService.SendAsync(request.To, request.Subject, request.Body, ct);
        return Result.Success();
    }
}
