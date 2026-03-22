namespace ELearning.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
    Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken ct = default);
}
