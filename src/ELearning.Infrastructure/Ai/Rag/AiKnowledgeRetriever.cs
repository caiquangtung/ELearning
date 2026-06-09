using System.Diagnostics;
using ELearning.Application.Common.Interfaces;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeRetriever(
    ApplicationDbContext context,
    IAiTextEmbeddingService embeddingService,
    IOptions<AiOptions> options)
    : IAiKnowledgeRetriever
{
    public async Task<AiKnowledgeRetrievalResult> RetrieveAsync(
        AiKnowledgeRetrievalRequest request,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var question = request.Question.Trim();
        var config = options.Value;
        var probe = embeddingService.Embed(question.Length == 0 ? "empty" : question);
        var minSimilarity = Math.Clamp(config.RagMinSimilarity, 0m, 1m);

        if (question.Length == 0)
            return EmptyResult(probe, minSimilarity, stopwatch);

        var maxChunks = Math.Clamp(config.RagMaxRetrievedChunks, 1, 8);
        var queryEmbedding = probe;
        if (!HasVectorSignal(queryEmbedding.Vector))
            return EmptyResult(queryEmbedding, minSimilarity, stopwatch);

        var accessibleCourseIds = await AiKnowledgeAccessPolicy.GetAccessiblePublishedCourseIdsAsync(
            context,
            request.UserId,
            request.UserRoles,
            request.CourseId,
            ct);

        if (accessibleCourseIds.Count == 0)
            return EmptyResult(queryEmbedding, minSimilarity, stopwatch);

        var candidates = await SearchVectorCandidatesAsync(
            accessibleCourseIds,
            PgVectorFormatter.ToVectorLiteral(queryEmbedding.Vector),
            Math.Max(maxChunks * 4, 12),
            ct);

        var citations = candidates
            .Where(x => x.Score >= minSimilarity)
            .Take(maxChunks)
            .Select(ToCitation)
            .ToList();

        return new AiKnowledgeRetrievalResult(
            citations,
            candidates.Count,
            citations.Count == 0 ? null : citations.Max(x => x.Score),
            minSimilarity,
            queryEmbedding.Provider,
            queryEmbedding.Model,
            queryEmbedding.Dimensions,
            stopwatch.ElapsedMilliseconds);
    }

    private async Task<List<VectorCandidate>> SearchVectorCandidatesAsync(
        IReadOnlyList<Guid> courseIds,
        string queryVector,
        int candidateLimit,
        CancellationToken ct)
    {
        var candidates = new List<VectorCandidate>();
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT
                    id,
                    course_id,
                    section_id,
                    lesson_id,
                    course_title,
                    section_title,
                    lesson_title,
                    text,
                    1 - (embedding_vector <=> CAST(@query_vector AS vector)) AS score
                FROM ai_knowledge_chunks
                WHERE course_id = ANY(@course_ids)
                    AND embedding_vector IS NOT NULL
                ORDER BY embedding_vector <=> CAST(@query_vector AS vector), course_title, chunk_index
                LIMIT @candidate_limit
                """;
            command.Parameters.Add(new NpgsqlParameter("query_vector", queryVector));
            command.Parameters.Add(new NpgsqlParameter("course_ids", courseIds.ToArray()));
            command.Parameters.Add(new NpgsqlParameter("candidate_limit", candidateLimit));

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                candidates.Add(new VectorCandidate(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    reader.IsDBNull(3) ? null : reader.GetGuid(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : reader.GetString(6),
                    reader.GetString(7),
                    Math.Round(Convert.ToDecimal(reader.GetValue(8), System.Globalization.CultureInfo.InvariantCulture), 4)));
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }

        return candidates;
    }

    private static AiKnowledgeRetrievalResult EmptyResult(
        AiTextEmbedding embedding,
        decimal minSimilarity,
        Stopwatch stopwatch) =>
        new(
            [],
            0,
            null,
            minSimilarity,
            embedding.Provider,
            embedding.Model,
            embedding.Dimensions,
            stopwatch.ElapsedMilliseconds);

    private static bool HasVectorSignal(IReadOnlyList<float> vector) =>
        vector.Any(value => Math.Abs(value) > 0.000001f);

    private static AiChatCitation ToCitation(VectorCandidate candidate) =>
        new(
            candidate.ChunkId,
            candidate.CourseId,
            candidate.SectionId,
            candidate.LessonId,
            candidate.CourseTitle,
            candidate.SectionTitle,
            candidate.LessonTitle,
            TrimSnippet(candidate.Text),
            candidate.Score);

    private static string TrimSnippet(string text)
    {
        var normalized = text.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 420 ? normalized : normalized[..420].TrimEnd() + "...";
    }

    private sealed record VectorCandidate(
        Guid ChunkId,
        Guid CourseId,
        Guid? SectionId,
        Guid? LessonId,
        string CourseTitle,
        string? SectionTitle,
        string? LessonTitle,
        string Text,
        decimal Score);
}
