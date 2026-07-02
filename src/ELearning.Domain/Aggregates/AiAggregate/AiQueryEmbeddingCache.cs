using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiQueryEmbeddingCache : AuditableAggregateRoot
{
    private AiQueryEmbeddingCache() { }

    public string QueryHash { get; private set; } = default!;
    public string NormalizedQuery { get; private set; } = default!;
    public string Provider { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public int Dimensions { get; private set; }
    public string EmbeddingJson { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }

    public static AiQueryEmbeddingCache Create(
        string queryHash,
        string normalizedQuery,
        string provider,
        string model,
        int dimensions,
        string embeddingJson,
        DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(queryHash))
            throw new DomainException("Query embedding cache hash is required.");
        if (string.IsNullOrWhiteSpace(normalizedQuery))
            throw new DomainException("Query embedding cache query is required.");
        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainException("Query embedding cache provider is required.");
        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Query embedding cache model is required.");
        if (dimensions <= 0)
            throw new DomainException("Query embedding cache dimensions are required.");
        if (string.IsNullOrWhiteSpace(embeddingJson))
            throw new DomainException("Query embedding cache embedding is required.");

        return new AiQueryEmbeddingCache
        {
            Id = Guid.NewGuid(),
            QueryHash = queryHash.Trim(),
            NormalizedQuery = normalizedQuery.Trim(),
            Provider = provider.Trim(),
            Model = model.Trim(),
            Dimensions = dimensions,
            EmbeddingJson = embeddingJson.Trim(),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Refresh(string embeddingJson, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(embeddingJson))
            throw new DomainException("Query embedding cache embedding is required.");

        EmbeddingJson = embeddingJson.Trim();
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
