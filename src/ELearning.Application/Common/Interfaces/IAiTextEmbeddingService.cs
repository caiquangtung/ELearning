namespace ELearning.Application.Common.Interfaces;

public interface IAiTextEmbeddingService
{
    Task<AiTextEmbedding> EmbedAsync(string text, CancellationToken ct = default);
}

public sealed record AiTextEmbedding(
    float[] Vector,
    string Provider,
    string Model,
    int Dimensions);
