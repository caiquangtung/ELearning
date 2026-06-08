using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeRetriever(
    ApplicationDbContext context,
    IAiEmbeddingService embeddingService,
    IOptions<AiOptions> options)
    : IAiKnowledgeRetriever
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<AiChatCitation>> RetrieveAsync(
        AiKnowledgeRetrievalRequest request,
        CancellationToken ct = default)
    {
        var question = request.Question.Trim();
        if (question.Length == 0)
            return [];

        var config = options.Value;
        var maxChunks = Math.Clamp(config.RagMaxRetrievedChunks, 1, 8);
        var minSimilarity = Math.Clamp(config.RagMinSimilarity, 0m, 1m);
        var queryEmbedding = embeddingService.Embed(question);

        var accessibleCourseIds = await AiKnowledgeAccessPolicy.GetAccessiblePublishedCourseIdsAsync(
            context,
            request.UserId,
            request.UserRoles,
            request.CourseId,
            ct);

        if (accessibleCourseIds.Count == 0)
            return [];

        var chunks = await context.AiKnowledgeChunks
            .AsNoTracking()
            .Where(x => accessibleCourseIds.Contains(x.CourseId))
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
            .ThenBy(x => x.Chunk.ChunkIndex)
            .Take(maxChunks)
            .Select(x => ToCitation(x.Chunk, x.Score))
            .ToList();
    }

    private static IReadOnlyDictionary<string, decimal> DeserializeEmbedding(string embeddingJson) =>
        JsonSerializer.Deserialize<Dictionary<string, decimal>>(embeddingJson, JsonOptions) ??
        new Dictionary<string, decimal>();

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
}
