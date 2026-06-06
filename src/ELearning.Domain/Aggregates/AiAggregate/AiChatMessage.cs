using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiChatMessage : AuditableEntity
{
    private AiChatMessage() { }

    public Guid SessionId { get; private set; }
    public string Role { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public string CitationsJson { get; private set; } = "[]";
    public string? Provider { get; private set; }
    public string? Model { get; private set; }
    public string? PromptVersion { get; private set; }
    public decimal? Confidence { get; private set; }
    public bool UsedContext { get; private set; }

    public static AiChatMessage User(Guid sessionId, string content) =>
        Create(sessionId, "User", content, "[]", null, null, null, null, false);

    public static AiChatMessage Assistant(
        Guid sessionId,
        string content,
        string citationsJson,
        string provider,
        string model,
        string promptVersion,
        decimal confidence,
        bool usedContext)
        => Create(sessionId, "Assistant", content, citationsJson, provider, model, promptVersion, confidence, usedContext);

    private static AiChatMessage Create(
        Guid sessionId,
        string role,
        string content,
        string citationsJson,
        string? provider,
        string? model,
        string? promptVersion,
        decimal? confidence,
        bool usedContext)
    {
        if (sessionId == Guid.Empty)
            throw new DomainException("Chat message session is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("Chat message role is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Chat message content is required.");
        if (confidence is < 0 or > 1)
            throw new DomainException("Chat message confidence must be between 0 and 1.");

        return new AiChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role.Trim(),
            Content = content.Trim(),
            CitationsJson = string.IsNullOrWhiteSpace(citationsJson) ? "[]" : citationsJson.Trim(),
            Provider = string.IsNullOrWhiteSpace(provider) ? null : provider.Trim(),
            Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim(),
            PromptVersion = string.IsNullOrWhiteSpace(promptVersion) ? null : promptVersion.Trim(),
            Confidence = confidence,
            UsedContext = usedContext,
            CreatedAt = DateTime.UtcNow
        };
    }
}
