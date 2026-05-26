using System.Text.Json;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AuditLogAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Security;

public sealed class AuditLogService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUser,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditLogService> logger)
    : IAuditLogService
{
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            var metadata = AuditMetadataSanitizer.Sanitize(entry.Metadata);
            var metadataJson = JsonSerializer.Serialize(metadata);

            dbContext.AuditLogs.Add(AuditLog.Create(
                entry.ActorUserId ?? currentUser.UserId,
                entry.Action,
                entry.TargetType,
                entry.TargetId,
                entry.Outcome,
                httpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault(),
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                httpContext?.Request.Headers.UserAgent.FirstOrDefault(),
                metadataJson,
                DateTime.UtcNow));

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Audit log write failed for action {Action}", entry.Action);
        }
    }
}
