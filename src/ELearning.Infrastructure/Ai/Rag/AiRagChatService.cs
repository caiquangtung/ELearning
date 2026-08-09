using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class AiRagChatService(
    ApplicationDbContext context,
    IAiKnowledgeRetriever knowledgeRetriever,
    OpenAiCompatibleChatClient chatClient,
    GoogleAiStudioChatClient? googleChatClient,
    OllamaChatClient? ollamaChatClient,
    IAiQueryRouter? queryRouter,
    IAiQueryRewriter? queryRewriter,
    IOptions<AiOptions> options,
    ILogger<AiRagChatService> logger)
    : IAiRagChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private const string DefaultRagNoContextPromptVersion = "rag-learning-assistant-no-context-v1";

    public async Task<AiChatSessionSummary> CreateSessionAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        Guid? courseId,
        string? title,
        CancellationToken ct = default)
    {
        Course? course = null;
        if (courseId.HasValue)
        {
            var accessibleCourseIds = await AiKnowledgeAccessPolicy.GetAccessiblePublishedCourseIdsAsync(
                context,
                userId,
                userRoles,
                courseId,
                ct);
            if (!accessibleCourseIds.Contains(courseId.Value))
                throw new KeyNotFoundException("Published course not found.");

            course = await context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId.Value && !c.IsDeleted && c.Status == CourseStatus.Published, ct);
            if (course is null)
                throw new KeyNotFoundException("Published course not found.");
        }

        var sessionTitle = string.IsNullOrWhiteSpace(title)
            ? course is null ? "AI Tutor" : $"AI Tutor: {course.Title}"
            : title.Trim();

        var session = AiChatSession.Create(userId, courseId, sessionTitle);
        await context.AiChatSessions.AddAsync(session, ct);
        await context.SaveChangesAsync(ct);
        return ToSummary(session);
    }

    public async Task<IReadOnlyList<AiAccessibleCourse>> GetAccessibleCoursesAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken ct = default)
    {
        var courseIds = await AiKnowledgeAccessPolicy.GetAccessiblePublishedCourseIdsAsync(
            context,
            userId,
            userRoles,
            null,
            ct);
        if (courseIds.Count == 0)
            return [];

        var courses = await context.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id) && !c.IsDeleted && c.Status == CourseStatus.Published)
            .OrderBy(c => c.Title)
            .Select(c => new AiAccessibleCourse(c.Id, c.Title))
            .ToListAsync(ct);

        return courses;
    }

    public async Task<IReadOnlyList<AiChatSessionSummary>> ListSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await context.AiChatSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(30)
            .ToListAsync(ct);

        return sessions.Select(ToSummary).ToList();
    }

    public async Task<IReadOnlyList<AiChatMessageItem>> GetMessagesAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken ct = default)
    {
        var ownsSession = await context.AiChatSessions
            .AnyAsync(x => x.Id == sessionId && x.UserId == userId, ct);
        if (!ownsSession)
            throw new KeyNotFoundException("Chat session not found.");

        var messages = await context.AiChatMessages
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return messages.Select(ToMessageItem).ToList();
    }

    public async Task<AiChatAnswer> SendMessageAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        Guid sessionId,
        string message,
        CancellationToken ct = default)
    {
        var session = await context.AiChatSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct);
        if (session is null)
            throw new KeyNotFoundException("Chat session not found.");

        var userMessage = AiChatMessage.User(session.Id, message);
        await context.AiChatMessages.AddAsync(userMessage, ct);

        var config = options.Value;
        var promptVersion = string.IsNullOrWhiteSpace(config.RagChatPromptVersion)
            ? "rag-learning-assistant-v1"
            : config.RagChatPromptVersion;

        var router = queryRouter ?? new AiQueryRouter(options);
        var routeResult = router.RouteQuery(message);

        AiChatAnswer answer;
        if (routeResult.SkipRetrieval)
        {
            if (!HasAiChatProvider(config))
            {
                logger.LogInformation(
                    "AI chat used local intent fallback. Intent={Intent}, Reason={Reason}",
                    routeResult.IntentName,
                    routeResult.Reason);

                answer = routeResult.Category == AiQueryIntentCategory.DirectGreeting
                    ? BuildGreetingAnswer(promptVersion, config.RagGreetingResponse)
                    : BuildNoContextAnswer(
                        message,
                        promptVersion,
                        routeResult.Category == AiQueryIntentCategory.OutOfScope
                            ? "Tôi là trợ lý học tập AI của hệ thống E-Learning, chỉ có thể hỗ trợ các câu hỏi liên quan đến nội dung khóa học và bài học. Bạn vui lòng đặt câu hỏi về khóa học nhé!"
                            : config.RagIrrelevantResponse);
            }
            else
            {
                answer = routeResult.Category == AiQueryIntentCategory.DirectGreeting
                    ? BuildGreetingAnswer(promptVersion, config.RagGreetingResponse)
                    : await GenerateAnswerAsync(message, [], promptVersion, ct);
            }
        }
        else
        {
            var recentMessages = await context.AiChatMessages
                .AsNoTracking()
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new AiChatMessageContext(x.Role, x.Content))
                .ToListAsync(ct);

            var rewrittenQuery = queryRewriter is not null
                ? await queryRewriter.RewriteQueryAsync(message, recentMessages, ct)
                : message;

            var retrieval = await knowledgeRetriever.RetrieveAsync(
                new AiKnowledgeRetrievalRequest(userId, userRoles, rewrittenQuery, session.CourseId),
                ct);

            logger.LogInformation(
                "AI chat used retrieval. OriginalQuery='{OriginalQuery}', RewrittenQuery='{RewrittenQuery}', Intent={Intent}, Citations={CitationCount}",
                message,
                rewrittenQuery,
                routeResult.IntentName,
                retrieval.Citations.Count);

            if (!HasSufficientRetrievalContext(retrieval.Citations, retrieval.MinAcceptedScore))
            {
                logger.LogInformation(
                    "AI chat retrieval quality gate refused generation. CandidateCount={CandidateCount}, MaxScore={MaxScore}, MinSimilarity={MinSimilarity}",
                    retrieval.RetrievedCount,
                    retrieval.MaxScore,
                    retrieval.MinAcceptedScore);

                answer = BuildNoContextAnswer(message, promptVersion, config.RagNoContextResponse);
            }
            else
            {
                answer = await GenerateAnswerAsync(rewrittenQuery, retrieval.Citations, promptVersion, ct);
            }
        }

        var citationsJson = JsonSerializer.Serialize(answer.Citations, JsonOptions);
        var assistantMessage = AiChatMessage.Assistant(
            session.Id,
            answer.Answer,
            citationsJson,
            answer.Provider,
            answer.Model,
            answer.PromptVersion,
            answer.Confidence,
            answer.UsedContext);

        await context.AiChatMessages.AddAsync(assistantMessage, ct);
        session.Touch();
        await context.SaveChangesAsync(ct);

        return answer with { MessageId = assistantMessage.Id };
    }

    private async Task<AiChatAnswer> GenerateAnswerAsync(
        string question,
        IReadOnlyList<AiChatCitation> citations,
        string promptVersion,
        CancellationToken ct)
    {
        var config = options.Value;
        var hasContext = citations.Count > 0;

        ValidateChatProviderConfiguration(config);

        if (citations.Count == 0 && !config.UsesGoogleAiStudioChatProvider() && !config.UsesOllamaChatProvider())
            return BuildNoContextAnswer(question, promptVersion, config.RagNoContextResponse);

        if (config.UsesOllamaChatProvider() && ollamaChatClient is not null)
        {
            try
            {
                var result = await ollamaChatClient.CompleteJsonAsync(
                    hasContext ? BuildSystemPrompt() : BuildNoContextSystemPrompt(),
                    hasContext ? BuildUserPrompt(question, citations) : question,
                    hasContext,
                    ct);

                if (hasContext)
                {
                    var answer = TryBuildProviderJsonAnswer(
                        question,
                        result.Provider,
                        result.Model,
                        result.Content,
                        result.TokenEstimate,
                        citations,
                        promptVersion);
                    if (answer is not null)
                        return answer;
                }
                else
                {
                    return new AiChatAnswer(
                        Guid.Empty,
                        result.Content.Trim(),
                        citations,
                        0m,
                        false,
                        result.Provider,
                        result.Model,
                        promptVersion,
                        result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(question, result.Content));
                }
            }
            catch (Exception ex) when (config.EnableLocalFallback)
            {
                logger.LogWarning(ex, "Ollama chat provider failed; falling back.");
            }
        }

        if (config.UsesGoogleAiStudioChatProvider() && googleChatClient is not null)
        {
            try
            {
                var result = await googleChatClient.CompleteJsonAsync(
                    hasContext ? BuildSystemPrompt() : BuildNoContextSystemPrompt(),
                    hasContext ? BuildUserPrompt(question, citations) : question,
                    ct);

                if (hasContext)
                {
                    var answer = TryBuildProviderJsonAnswer(
                        question,
                        result.Provider,
                        result.Model,
                        result.Content,
                        result.TokenEstimate,
                        citations,
                        promptVersion);
                    if (answer is not null)
                        return answer;

                    throw new InvalidOperationException("Google AI Studio chat provider returned an invalid RAG JSON answer.");
                }

                return new AiChatAnswer(
                    Guid.Empty,
                    result.Content.Trim(),
                    citations,
                    Math.Round(Math.Clamp(hasContext ? 0.85m : 0m, 0m, 1m), 2),
                    hasContext,
                    result.Provider,
                    result.Model,
                    promptVersion,
                    result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(question, result.Content));
            }
            catch (Exception ex) when (config.EnableLocalFallback)
            {
                logger.LogWarning(ex, "Google AI Studio chat provider failed; falling back.");
            }
        }

        if (config.EnableOpenAiCompatibleFallback &&
            config.UsesOpenAiCompatibleProvider() &&
            !string.IsNullOrWhiteSpace(config.ApiKey) &&
            !string.IsNullOrWhiteSpace(config.ResolveChatModel()))
        {
            try
            {
                var result = await chatClient.CompleteJsonAsync(
                    BuildSystemPrompt(),
                    BuildUserPrompt(question, citations),
                    ct);
                var answer = TryBuildProviderJsonAnswer(
                    question,
                    result.Provider,
                    result.Model,
                    result.Content,
                    result.TokenEstimate,
                    citations,
                    promptVersion);
                if (answer is not null)
                    return answer;
            }
            catch (Exception ex) when (config.EnableLocalFallback)
            {
                logger.LogWarning(ex, "RAG chat provider failed; falling back to extractive answer.");
            }
        }

        if (config.UsesOllamaChatProvider() && !config.EnableLocalFallback)
            throw new InvalidOperationException("Ollama chat provider failed and local fallback is disabled.");

        if (config.UsesGoogleAiStudioChatProvider() && !config.EnableLocalFallback)
            throw new InvalidOperationException("Google AI Studio chat provider failed and local fallback is disabled.");

        if (config.UsesOpenAiCompatibleProvider() && !config.EnableOpenAiCompatibleFallback)
            throw new InvalidOperationException("OpenAI-compatible chat provider is disabled by configuration.");

        if (hasContext)
            return BuildExtractiveAnswer(question, citations, promptVersion);

        return BuildNoContextAnswer(question, promptVersion, config.RagNoContextResponse);
    }

    internal static bool HasSufficientRetrievalContext(
        IReadOnlyList<AiChatCitation> citations,
        decimal minSimilarity) =>
        citations.Count > 0 && citations.Max(x => x.Score) >= minSimilarity;

    internal static AiChatAnswer? TryBuildProviderJsonAnswer(
        string question,
        string provider,
        string model,
        string content,
        int? tokenEstimate,
        IReadOnlyList<AiChatCitation> citations,
        string promptVersion)
    {
        try
        {
            var providerAnswer = JsonSerializer.Deserialize<RagProviderAnswer>(
                OpenAiCompatibleJson.ExtractObject(content),
                JsonOptions);

            if (string.IsNullOrWhiteSpace(providerAnswer?.Answer))
                return null;

            return new AiChatAnswer(
                Guid.Empty,
                providerAnswer.Answer.Trim(),
                citations,
                Math.Round(Math.Clamp(providerAnswer.Confidence, 0m, 1m), 2),
                true,
                provider,
                model,
                promptVersion,
                tokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(question, content));
        }
        catch (JsonException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static void ValidateChatProviderConfiguration(AiOptions config)
    {
        if (config.UsesOllamaChatProvider())
        {
            if (string.IsNullOrWhiteSpace(config.OllamaBaseUrl))
                throw new InvalidOperationException("Ai:OllamaBaseUrl is required when Ai:RagChatProvider is Ollama.");

            if (string.IsNullOrWhiteSpace(config.OllamaModel))
                throw new InvalidOperationException("Ai:OllamaModel is required when Ai:RagChatProvider is Ollama.");
        }

        if (config.UsesGoogleAiStudioChatProvider())
        {
            if (string.IsNullOrWhiteSpace(config.ResolveRagEmbeddingApiKey()))
                throw new InvalidOperationException("Ai:ApiKey or Ai:RagEmbeddingApiKey is required when Ai:RagChatProvider is GoogleAiStudio.");

            if (string.IsNullOrWhiteSpace(config.ResolveChatModel()))
                throw new InvalidOperationException("Ai:ChatModel or Ai:RagChatModel is required when Ai:RagChatProvider is GoogleAiStudio.");
        }

        if (config.UsesOpenAiCompatibleProvider())
        {
            if (string.IsNullOrWhiteSpace(config.ApiKey))
                throw new InvalidOperationException("Ai:ApiKey is required when Ai:RagChatProvider is OpenAiCompatible.");

            if (string.IsNullOrWhiteSpace(config.ResolveChatModel()))
                throw new InvalidOperationException("Ai:ChatModel or Ai:RagChatModel is required when Ai:RagChatProvider is OpenAiCompatible.");
        }
    }

    private static bool HasAiChatProvider(AiOptions config) =>
        config.UsesOpenAiCompatibleProvider() || config.UsesGoogleAiStudioChatProvider() || config.UsesOllamaChatProvider();

    internal static AiChatAnswer BuildGreetingAnswer(string promptVersion, string? response = null) =>
        new(
            Guid.Empty,
            NormalizeConfiguredResponse(
                response,
                "Hello! I'm your AI learning assistant. Ask me anything about your course content and I will help you find the answer."),
            [],
            0m,
            false,
            "Local",
            "intent-gate-v1",
            promptVersion,
            OpenAiCompatibleJson.EstimateTokens("Hello!"));

    internal static AiChatAnswer BuildNoContextAnswer(
        string question,
        string promptVersion,
        string? response = null)
    {
        var tokens = OpenAiCompatibleJson.EstimateTokens(question);
        return new AiChatAnswer(
            Guid.Empty,
            NormalizeConfiguredResponse(
                response,
                "I don't have enough course material to answer that. Try rephrasing your question to focus on a specific lesson or topic in your course."),
            [],
            0m,
            false,
            "Local",
            "no-context-rag-v1",
            promptVersion,
            tokens);
    }

    private static string NormalizeConfiguredResponse(string? response, string fallback) =>
        string.IsNullOrWhiteSpace(response) ? fallback : response.Trim();

    internal static AiChatAnswer BuildExtractiveAnswer(
        string question,
        IReadOnlyList<AiChatCitation> citations,
        string promptVersion)
    {
        var selected = citations.Take(2).ToList();
        var snippets = string.Join(" ", selected.Select(x => x.Snippet.TrimEnd('.', ' ') + "."));
        var answer = $"Based on the course material: {snippets}";

        return new AiChatAnswer(
            Guid.Empty,
            answer,
            citations,
            Math.Round(Math.Min(0.85m, citations.Max(x => x.Score)), 2),
            true,
            "Local",
            "extractive-rag-v1",
            promptVersion,
            OpenAiCompatibleJson.EstimateTokens(question, answer));
    }

    private static string BuildSystemPrompt() =>
        PromptTemplateStore.LoadSystemPrompt(
            "rag-learning-assistant-v1",
            """
            You are an LMS learning assistant. Answer only from the provided course excerpts.
            If the excerpts do not answer the question, return: {"answer":"I don't have enough course material to answer that.","confidence":0}
            Return only JSON with shape {"answer":"...","confidence":0.0}.
            Do not invent citations or facts outside the provided excerpts.
            """);

    private static string BuildNoContextSystemPrompt() =>
        PromptTemplateStore.LoadSystemPrompt(
            DefaultRagNoContextPromptVersion,
            """
            You are an AI learning assistant named Elearning Bot.
            Answer naturally and concisely. You can answer questions about yourself, general topics, or guide users to ask about course content.
            If the question seems clearly about specific course material that is not available, politely say you don't have enough course material for that topic.
            """);

    private static string BuildUserPrompt(string question, IReadOnlyList<AiChatCitation> citations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Excerpts from the course materials:");
        sb.AppendLine();
        for (int i = 0; i < citations.Count; i++)
        {
            var c = citations[i];
            sb.AppendLine($"[Excerpt #{i + 1}]");
            sb.AppendLine($"Course: {c.CourseTitle}");
            if (!string.IsNullOrWhiteSpace(c.SectionTitle))
                sb.AppendLine($"Section: {c.SectionTitle}");
            if (!string.IsNullOrWhiteSpace(c.LessonTitle))
                sb.AppendLine($"Lesson: {c.LessonTitle}");
            sb.AppendLine($"Content: {c.Snippet.Trim()}");
            sb.AppendLine();
        }

        sb.AppendLine($"Question: {question}");
        return sb.ToString();
    }

    private static AiChatSessionSummary ToSummary(AiChatSession session) =>
        new(session.Id, session.Title, session.CourseId, session.CreatedAt, session.UpdatedAt);

    private static AiChatMessageItem ToMessageItem(AiChatMessage message) =>
        new(
            message.Id,
            message.Role,
            message.Content,
            DeserializeCitations(message.CitationsJson),
            message.Provider,
            message.Model,
            message.PromptVersion,
            message.Confidence,
            message.UsedContext,
            message.CreatedAt);

    private static IReadOnlyList<AiChatCitation> DeserializeCitations(string citationsJson) =>
        JsonSerializer.Deserialize<IReadOnlyList<AiChatCitation>>(citationsJson, JsonOptions) ?? [];

    private sealed record RagProviderAnswer(string? Answer, decimal Confidence);
}
