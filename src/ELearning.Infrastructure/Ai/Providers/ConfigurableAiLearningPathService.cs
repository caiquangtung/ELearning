using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class ConfigurableAiLearningPathService(
    LocalLearningPathService local,
    OpenAiCompatibleLearningPathService openAiCompatible,
    IOptions<AiOptions> options,
    ILogger<ConfigurableAiLearningPathService> logger)
    : IAiLearningPathService
{
    public string CacheVariant
    {
        get
        {
            var config = options.Value;
            return config.UsesOpenAiCompatibleProvider()
                ? openAiCompatible.CacheVariant
                : local.CacheVariant;
        }
    }

    public async Task<AiLearningPathDraft> GenerateAsync(AiLearningPathRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.UsesOpenAiCompatibleProvider())
            return await local.GenerateAsync(request, ct);

        try
        {
            return await openAiCompatible.GenerateAsync(request, ct);
        }
        catch (Exception ex) when (config.EnableLocalFallback)
        {
            logger.LogWarning(ex, "OpenAI-compatible learning path generation failed; falling back to local provider.");
            return await local.GenerateAsync(request, ct);
        }
    }
}
