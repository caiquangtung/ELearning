namespace ELearning.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; init; } = "Local";
    public string Model { get; init; } = "local-deterministic-v1";
    public string BaseUrl { get; init; } = "https://api.openai.com/v1";
    public string ApiKey { get; init; } = "";
    public string ChatModel { get; init; } = "";
    public string RagChatProvider { get; init; } = "Local";
    // Recommended: For Google AI Studio chat, use "gemini-2.5-flash" or "gemini-3.5-flash" for better quality and free tier rate limits.
    public string RagChatModel { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxOutputTokens { get; init; } = 1200;
    public int MaxRetries { get; init; } = 2;
    private readonly bool? _enableLocalFallback;
    public bool EnableLocalFallback
    {
        get => _enableLocalFallback ?? FallbackToLocal;
        init => _enableLocalFallback = value;
    }
    public bool FallbackToLocal { get; init; } = true;
    public bool EnableOpenAiCompatibleFallback { get; init; } = true;
    public string OllamaBaseUrl { get; init; } = "http://localhost:11434";
    public string OllamaModel { get; init; } = "qwen2.5:7b";
    public int OllamaTimeoutSeconds { get; init; } = 120;
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
    public decimal RagMinSimilarity { get; init; } = 0.50m;
    public int RagMaxContextCharacters { get; init; } = 2400;
    public int RagCandidateMultiplier { get; init; } = 8;
    public int RagReindexPollSeconds { get; init; } = 5;
    public int MaxSourceCharacters { get; init; } = 12000;
    public bool RagEnableIntentGating { get; init; } = true;
    public string RagGreetingResponse { get; init; } = "Hello! I'm your AI learning assistant. Ask me anything about your course content and I will help you find the answer.";
    public string RagIrrelevantResponse { get; init; } = "I don't have enough course material to answer that. Try rephrasing your question to focus on a specific lesson or topic in your course.";
    public string RagNoContextResponse { get; init; } = "I don't have enough course material to answer that. Try asking about a specific topic covered in your course.";

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

    public bool UsesGoogleAiStudioChatProvider() =>
        RagChatProvider.Equals("GoogleAiStudio", StringComparison.OrdinalIgnoreCase);

    public bool UsesOllamaChatProvider() =>
        RagChatProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase);

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

        if (!string.IsNullOrWhiteSpace(RagChatModel))
            return RagChatModel.Trim();

        if (UsesOllamaChatProvider())
            return string.IsNullOrWhiteSpace(OllamaModel) ? "qwen2.5:7b" : OllamaModel.Trim();

        return UsesOpenAiCompatibleProvider()
            ? ""
            : string.IsNullOrWhiteSpace(Model) ? "local-deterministic-v1" : Model.Trim();
    }
}
