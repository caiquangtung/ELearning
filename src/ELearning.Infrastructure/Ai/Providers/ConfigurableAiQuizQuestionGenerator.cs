using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class ConfigurableAiQuizQuestionGenerator(
    LocalQuizQuestionGenerator local,
    OpenAiCompatibleQuizQuestionGenerator openAiCompatible,
    IOptions<AiOptions> options,
    ILogger<ConfigurableAiQuizQuestionGenerator> logger)
    : IAiQuizQuestionGenerator
{
    public async Task<AiQuizQuestionGenerationResult> GenerateAsync(
        AiQuizQuestionGenerationRequest request,
        CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.UsesOpenAiCompatibleProvider())
            return await local.GenerateAsync(request, ct);

        try
        {
            return await openAiCompatible.GenerateAsync(request, ct);
        }
        catch (Exception ex) when (config.FallbackToLocal)
        {
            logger.LogWarning(ex, "OpenAI-compatible quiz generation failed; falling back to local provider.");
            return await local.GenerateAsync(request, ct);
        }
    }
}
