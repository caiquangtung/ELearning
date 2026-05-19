using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Notifications;

public sealed class NoOpEmailService(ILogger<NoOpEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("NoOp email sent to {Recipient} with subject {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken ct = default)
    {
        logger.LogInformation("NoOp templated email sent to {Recipient} with template {Template}", to, templateName);
        return Task.CompletedTask;
    }
}
