namespace ELearning.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; init; } = "Local";
    public string Model { get; init; } = "local-deterministic-v1";
    public string BaseUrl { get; init; } = "https://api.openai.com/v1";
    public string ApiKey { get; init; } = "";
    public string ChatModel { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxOutputTokens { get; init; } = 1200;
    public int MaxRetries { get; init; } = 2;
    public bool FallbackToLocal { get; init; } = true;
    public string QuizQuestionPromptVersion { get; init; } = "quiz-question-generator-v1";
    public string EssayGradingPromptVersion { get; init; } = "essay-grading-v1";
    public string LearningPathPromptVersion { get; init; } = "learning-path-generator-v1";
    public string RagChatPromptVersion { get; init; } = "rag-learning-assistant-v1";
    public string RagEmbeddingProvider { get; init; } = "Local";
    public string RagEmbeddingBaseUrl { get; init; } = "";
    public string RagEmbeddingApiKey { get; init; } = "";
    public string RagEmbeddingModel { get; init; } = "";
    public int RagEmbeddingDimensions { get; init; } = 768;
    public int RagEmbeddingTimeoutSeconds { get; init; } = 30;
    public int RagEmbeddingMaxRetries { get; init; } = 2;
    public string RagEmbeddingFailureMode { get; init; } = "FullTextFallback";
    public int RagQueryEmbeddingCacheTtlDays { get; init; } = 30;
    public bool RagAutoReindexEnabled { get; init; } = true;
    public int RagMaxRetrievedChunks { get; init; } = 4;
    public decimal RagMinSimilarity { get; init; } = 0.05m;
    public int RagMaxContextCharacters { get; init; } = 2400;
    public int RagCandidateMultiplier { get; init; } = 8;
    public int RagReindexPollSeconds { get; init; } = 5;
    public int MaxSourceCharacters { get; init; } = 12000;

    public bool UsesOpenAiCompatibleProvider() =>
        Provider.Equals("OpenAiCompatible", StringComparison.OrdinalIgnoreCase);

    public bool UsesOpenAiCompatibleRagEmbeddingProvider() =>
        RagEmbeddingProvider.Equals("OpenAiCompatible", StringComparison.OrdinalIgnoreCase);

    public bool UsesGoogleAiStudioRagEmbeddingProvider() =>
        RagEmbeddingProvider.Equals("GoogleAiStudio", StringComparison.OrdinalIgnoreCase);

    public bool UsesRemoteRagEmbeddingProvider() =>
        UsesOpenAiCompatibleRagEmbeddingProvider() || UsesGoogleAiStudioRagEmbeddingProvider();

    public bool UsesFullTextEmbeddingFailureFallback() =>
        RagEmbeddingFailureMode.Equals("FullTextFallback", StringComparison.OrdinalIgnoreCase);

    public string ResolveRagEmbeddingBaseUrl() =>
        string.IsNullOrWhiteSpace(RagEmbeddingBaseUrl)
            ? string.IsNullOrWhiteSpace(BaseUrl) ? "https://api.openai.com/v1" : BaseUrl.Trim()
            : RagEmbeddingBaseUrl.Trim();

    public string ResolveRagEmbeddingApiKey() =>
        string.IsNullOrWhiteSpace(RagEmbeddingApiKey)
            ? ApiKey.Trim()
            : RagEmbeddingApiKey.Trim();

    public string ResolveGoogleAiStudioRagEmbeddingBaseUrl() =>
        string.IsNullOrWhiteSpace(RagEmbeddingBaseUrl)
            ? "https://generativelanguage.googleapis.com/v1beta"
            : RagEmbeddingBaseUrl.Trim();

    public string ResolveRagEmbeddingModel()
    {
        if (!string.IsNullOrWhiteSpace(RagEmbeddingModel))
            return RagEmbeddingModel.Trim();

        return UsesGoogleAiStudioRagEmbeddingProvider()
            ? "gemini-embedding-2"
            : "";
    }

    public string ResolveChatModel()
    {
        if (!string.IsNullOrWhiteSpace(ChatModel))
            return ChatModel.Trim();

        return UsesOpenAiCompatibleProvider()
            ? ""
            : string.IsNullOrWhiteSpace(Model) ? "local-deterministic-v1" : Model.Trim();
    }
}
