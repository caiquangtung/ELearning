namespace ELearning.Application.Common.Interfaces;

public interface IAiTextEmbeddingService
{
    AiTextEmbedding Embed(string text);
}

public sealed record AiTextEmbedding(
    float[] Vector,
    string Provider,
    string Model,
    int Dimensions);
