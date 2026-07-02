namespace ELearning.Application.Common.Interfaces;

public interface IAiTextEmbeddingService
{
    Task<AiTextEmbedding> EmbedAsync(AiTextEmbeddingRequest request, CancellationToken ct = default);
}

public enum AiTextEmbeddingPurpose
{
    RetrievalDocument,
    RetrievalQuery,
    StatusProbe
}

public sealed record AiTextEmbeddingRequest(
    string Text,
    AiTextEmbeddingPurpose Purpose,
    string? Title = null);

public sealed record AiTextEmbedding(
    float[] Vector,
    string Provider,
    string Model,
    int Dimensions);
