using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AuditLogAggregate;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string TargetType { get; private set; } = default!;
    public string? TargetId { get; private set; }
    public string Outcome { get; private set; } = default!;
    public string? CorrelationId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string MetadataJson { get; private set; } = "{}";
    public DateTime CreatedAtUtc { get; private set; }

    public static AuditLog Create(
        Guid? actorUserId,
        string action,
        string targetType,
        string? targetId,
        string outcome,
        string? correlationId,
        string? ipAddress,
        string? userAgent,
        string metadataJson,
        DateTime createdAtUtc)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = action.Trim(),
            TargetType = targetType.Trim(),
            TargetId = string.IsNullOrWhiteSpace(targetId) ? null : targetId.Trim(),
            Outcome = outcome.Trim(),
            CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim(),
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson,
            CreatedAtUtc = createdAtUtc
        };
    }
}
