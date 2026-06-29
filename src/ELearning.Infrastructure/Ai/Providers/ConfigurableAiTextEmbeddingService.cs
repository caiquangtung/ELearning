using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class ConfigurableAiTextEmbeddingService(
    LocalDenseTextEmbeddingService local,
    OpenAiCompatibleTextEmbeddingService openAiCompatible,
    IOptions<AiOptions> options,
    ILogger<ConfigurableAiTextEmbeddingService> logger)
    : IAiTextEmbeddingService
{
    public async Task<AiTextEmbedding> EmbedAsync(string text, CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.UsesOpenAiCompatibleRagEmbeddingProvider())
            return await local.EmbedAsync(text, ct);

        try
        {
            return await openAiCompatible.EmbedAsync(text, ct);
        }
        catch (Exception ex) when (config.FallbackToLocal)
        {
            logger.LogWarning(ex, "OpenAI-compatible RAG embedding provider failed; falling back to local dense embedding.");
            return await local.EmbedAsync(text, ct);
        }
    }
}
