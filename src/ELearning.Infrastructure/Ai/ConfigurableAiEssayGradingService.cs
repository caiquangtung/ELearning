using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class ConfigurableAiEssayGradingService(
    LocalEssayGradingService local,
    OpenAiCompatibleEssayGradingService openAiCompatible,
    IOptions<AiOptions> options,
    ILogger<ConfigurableAiEssayGradingService> logger)
    : IAiEssayGradingService
{
    public async Task<AiEssayGradingResult> SuggestAsync(AiEssayGradingRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.UsesOpenAiCompatibleProvider())
            return await local.SuggestAsync(request, ct);

        try
        {
            return await openAiCompatible.SuggestAsync(request, ct);
        }
        catch (Exception ex) when (config.FallbackToLocal)
        {
            logger.LogWarning(ex, "OpenAI-compatible essay grading failed; falling back to local provider.");
            return await local.SuggestAsync(request, ct);
        }
    }
}
