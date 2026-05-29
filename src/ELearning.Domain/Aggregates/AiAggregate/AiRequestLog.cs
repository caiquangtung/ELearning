using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiRequestLog : AuditableAggregateRoot
{
    private AiRequestLog() { }

    public Guid? UserId { get; private set; }
    public string Feature { get; private set; } = default!;
    public string Provider { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public string PromptVersion { get; private set; } = default!;
    public string InputHash { get; private set; } = default!;
    public int? TokenEstimate { get; private set; }
    public AiRequestStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static AiRequestLog Succeeded(
        Guid? userId,
        string feature,
        string provider,
        string model,
        string promptVersion,
        string inputHash,
        int? tokenEstimate)
        => Create(userId, feature, provider, model, promptVersion, inputHash, tokenEstimate, AiRequestStatus.Succeeded, null);

    public static AiRequestLog Failed(
        Guid? userId,
        string feature,
        string provider,
        string model,
        string promptVersion,
        string inputHash,
        string errorMessage)
        => Create(userId, feature, provider, model, promptVersion, inputHash, null, AiRequestStatus.Failed, errorMessage);

    private static AiRequestLog Create(
        Guid? userId,
        string feature,
        string provider,
        string model,
        string promptVersion,
        string inputHash,
        int? tokenEstimate,
        AiRequestStatus status,
        string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(feature))
            throw new DomainException("AI feature is required.");
        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainException("AI provider is required.");
        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("AI model is required.");
        if (string.IsNullOrWhiteSpace(promptVersion))
            throw new DomainException("AI prompt version is required.");
        if (string.IsNullOrWhiteSpace(inputHash))
            throw new DomainException("AI input hash is required.");

        return new AiRequestLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Feature = feature.Trim(),
            Provider = provider.Trim(),
            Model = model.Trim(),
            PromptVersion = promptVersion.Trim(),
            InputHash = inputHash.Trim(),
            TokenEstimate = tokenEstimate,
            Status = status,
            ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
