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
    IAiEmbeddingService embeddingService,
    OpenAiCompatibleChatClient chatClient,
    IOptions<AiOptions> options,
    ILogger<AiRagChatService> logger)
    : IAiRagChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiChatSessionSummary> CreateSessionAsync(
        Guid userId,
        Guid? courseId,
        string? title,
        CancellationToken ct = default)
    {
        Course? course = null;
        if (courseId.HasValue)
        {
            course = await context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId.Value && c.Status == CourseStatus.Published, ct);
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

        var citations = await RetrieveAsync(message, session.CourseId, ct);
        var answer = await GenerateAnswerAsync(message, citations, ct);
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

    private async Task<IReadOnlyList<AiChatCitation>> RetrieveAsync(
        string question,
        Guid? courseId,
        CancellationToken ct)
    {
        var config = options.Value;
        var maxChunks = Math.Clamp(config.RagMaxRetrievedChunks, 1, 8);
        var minSimilarity = Math.Clamp(config.RagMinSimilarity, 0m, 1m);
        var queryEmbedding = embeddingService.Embed(question);

        var publishedCourseIds = await context.Courses
            .Where(c => c.Status == CourseStatus.Published)
            .Where(c => !courseId.HasValue || c.Id == courseId.Value)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (publishedCourseIds.Count == 0)
            return [];

        var chunks = await context.AiKnowledgeChunks
            .AsNoTracking()
            .Where(x => publishedCourseIds.Contains(x.CourseId))
            .ToListAsync(ct);

        return chunks
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = embeddingService.CosineSimilarity(queryEmbedding, DeserializeEmbedding(chunk.EmbeddingJson))
            })
            .Where(x => x.Score >= minSimilarity)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Chunk.CourseTitle)
            .Take(maxChunks)
            .Select(x => ToCitation(x.Chunk, x.Score))
            .ToList();
    }

    private async Task<AiChatAnswer> GenerateAnswerAsync(
        string question,
        IReadOnlyList<AiChatCitation> citations,
        CancellationToken ct)
    {
        var config = options.Value;
        var promptVersion = string.IsNullOrWhiteSpace(config.RagChatPromptVersion)
            ? "rag-learning-assistant-v1"
            : config.RagChatPromptVersion;

        if (citations.Count == 0)
        {
            return new AiChatAnswer(
                Guid.Empty,
                "I don't have enough course material to answer that.",
                [],
                0m,
                false,
                "Local",
                "extractive-rag-v1",
                promptVersion,
                OpenAiCompatibleJson.EstimateTokens(question));
        }

        if (config.UsesOpenAiCompatibleProvider() &&
            !string.IsNullOrWhiteSpace(config.ApiKey) &&
            !string.IsNullOrWhiteSpace(config.ResolveChatModel()))
        {
            try
            {
                var result = await chatClient.CompleteJsonAsync(
                    BuildSystemPrompt(),
                    BuildUserPrompt(question, citations),
                    ct);
                var providerAnswer = JsonSerializer.Deserialize<RagProviderAnswer>(
                    OpenAiCompatibleJson.ExtractObject(result.Content),
                    JsonOptions);

                if (!string.IsNullOrWhiteSpace(providerAnswer?.Answer))
                {
                    return new AiChatAnswer(
                        Guid.Empty,
                        providerAnswer.Answer.Trim(),
                        citations,
                        Math.Round(Math.Clamp(providerAnswer.Confidence, 0m, 1m), 2),
                        true,
                        result.Provider,
                        result.Model,
                        promptVersion,
                        result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(question, result.Content));
                }
            }
            catch (Exception ex) when (config.FallbackToLocal)
            {
                logger.LogWarning(ex, "RAG chat provider failed; falling back to extractive answer.");
            }
        }

        return BuildExtractiveAnswer(question, citations, promptVersion);
    }

    private static AiChatAnswer BuildExtractiveAnswer(
        string question,
        IReadOnlyList<AiChatCitation> citations,
        string promptVersion)
    {
        var selected = citations.Take(2).ToList();
        var answer = "Based on the course material: " +
            string.Join(" ", selected.Select(x => x.Snippet.TrimEnd('.', ' ') + "."));

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
        """
        You are an LMS learning assistant. Answer only from the provided course excerpts.
        If the excerpts do not answer the question, return: {"answer":"I don't have enough course material to answer that.","confidence":0}
        Return only JSON with shape {"answer":"...","confidence":0.0}.
        Do not invent citations or facts outside the provided excerpts.
        """;

    private static string BuildUserPrompt(string question, IReadOnlyList<AiChatCitation> citations)
    {
        var payload = new
        {
            Question = question,
            Excerpts = citations.Select((citation, index) => new
            {
                Index = index + 1,
                citation.CourseTitle,
                citation.SectionTitle,
                citation.LessonTitle,
                citation.Snippet
            })
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static IReadOnlyDictionary<string, decimal> DeserializeEmbedding(string embeddingJson) =>
        JsonSerializer.Deserialize<Dictionary<string, decimal>>(embeddingJson, JsonOptions) ??
        new Dictionary<string, decimal>();

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

    private static AiChatCitation ToCitation(AiKnowledgeChunk chunk, decimal score) =>
        new(
            chunk.Id,
            chunk.CourseId,
            chunk.SectionId,
            chunk.LessonId,
            chunk.CourseTitle,
            chunk.SectionTitle,
            chunk.LessonTitle,
            TrimSnippet(chunk.Text),
            Math.Round(score, 4));

    private static string TrimSnippet(string text)
    {
        var normalized = text.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 420 ? normalized : normalized[..420].TrimEnd() + "...";
    }

    private sealed record RagProviderAnswer(string? Answer, decimal Confidence);
}
