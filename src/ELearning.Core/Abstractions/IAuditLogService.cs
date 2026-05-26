namespace ELearning.Core.Abstractions;

public interface IAuditLogService
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default);
}

public sealed record AuditLogEntry(
    string Action,
    string TargetType,
    string? TargetId,
    string Outcome,
    IReadOnlyDictionary<string, string>? Metadata = null,
    Guid? ActorUserId = null);
