using System.Diagnostics;
using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ELearning.Infrastructure.Ai;

public sealed partial class AiKnowledgeRetriever(
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
        var minSimilarity = Math.Clamp(config.RagMinSimilarity, 0m, 1m);

        if (question.Length == 0)
            return EmptyResult(EmptyEmbedding(), minSimilarity, stopwatch);

        var maxChunks = Math.Clamp(config.RagMaxRetrievedChunks, 1, 8);
        var candidateLimit = Math.Max(maxChunks * Math.Clamp(config.RagCandidateMultiplier, 4, 20), 24);
        var contextBudget = Math.Clamp(config.RagMaxContextCharacters, 800, 8000);

        var accessibleCourseIds = await AiKnowledgeAccessPolicy.GetAccessiblePublishedCourseIdsAsync(
            context,
            request.UserId,
            request.UserRoles,
            request.CourseId,
            ct);

        if (accessibleCourseIds.Count == 0)
            return EmptyResult(EmptyEmbedding(), minSimilarity, stopwatch);

        AiTextEmbedding queryEmbedding;
        List<VectorCandidate> candidates;
        try
        {
            queryEmbedding = await embeddingService.EmbedAsync(
                new AiTextEmbeddingRequest(question, AiTextEmbeddingPurpose.RetrievalQuery),
                ct);
            if (!HasVectorSignal(queryEmbedding.Vector))
                return EmptyResult(queryEmbedding, minSimilarity, stopwatch);

            candidates = await SearchVectorCandidatesAsync(
                accessibleCourseIds,
                PgVectorFormatter.ToVectorLiteral(queryEmbedding.Vector),
                candidateLimit,
                ct);
        }
        catch (Exception) when (ShouldUseFullTextFallback(config))
        {
            queryEmbedding = new AiTextEmbedding([], "PostgreSql", "full-text-fallback-v1", 0);
            candidates = await SearchFullTextCandidatesAsync(accessibleCourseIds, question, candidateLimit, ct);
            minSimilarity = 0.01m;
        }

        var citations = BuildCitations(question, candidates, minSimilarity, maxChunks, contextBudget);

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

    private static bool ShouldUseFullTextFallback(AiOptions config) =>
        config.UsesGoogleAiStudioRagEmbeddingProvider() && config.UsesFullTextEmbeddingFailureFallback();

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
                    source_type,
                    chunk_index,
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
                    reader.GetInt32(8),
                    reader.GetString(9),
                    Math.Round(Convert.ToDecimal(reader.GetValue(10), System.Globalization.CultureInfo.InvariantCulture), 4)));
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }

        return candidates;
    }

    private async Task<List<VectorCandidate>> SearchFullTextCandidatesAsync(
        IReadOnlyList<Guid> courseIds,
        string question,
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
            var queryTerms = TokenizeForLexicalScore(question);
            string queryText;
            string tsQueryFunction;

            if (queryTerms.Count > 0)
            {
                queryText = string.Join(" | ", queryTerms.Select(t => $"'{t.Replace("'", "''")}'"));
                tsQueryFunction = "to_tsquery";
            }
            else
            {
                queryText = question;
                tsQueryFunction = "websearch_to_tsquery";
            }

            command.CommandText =
                $"""
                WITH query AS (
                    SELECT {tsQueryFunction}('simple', @question_query) AS value
                )
                SELECT
                    c.id,
                    c.course_id,
                    c.section_id,
                    c.lesson_id,
                    c.course_title,
                    c.section_title,
                    c.lesson_title,
                    c.source_type,
                    c.chunk_index,
                    c.text,
                    ts_rank_cd(
                        to_tsvector('simple', concat_ws(' ', c.course_title, c.section_title, c.lesson_title, c.text)),
                        query.value) AS score
                FROM ai_knowledge_chunks c
                CROSS JOIN query
                WHERE c.course_id = ANY(@course_ids)
                    AND to_tsvector('simple', concat_ws(' ', c.course_title, c.section_title, c.lesson_title, c.text)) @@ query.value
                ORDER BY score DESC, c.course_title, c.chunk_index
                LIMIT @candidate_limit
                """;
            command.Parameters.Add(new NpgsqlParameter("question_query", queryText));
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
                    reader.GetInt32(8),
                    reader.GetString(9),
                    Math.Round(Math.Clamp(Convert.ToDecimal(reader.GetValue(10), System.Globalization.CultureInfo.InvariantCulture), 0m, 1m), 4)));
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

    private static AiTextEmbedding EmptyEmbedding() =>
        new([], "None", "none", 0);

    private static bool HasVectorSignal(IReadOnlyList<float> vector) =>
        vector.Any(value => Math.Abs(value) > 0.000001f);

    internal static IReadOnlyList<AiChatCitation> BuildCitations(
        string question,
        IReadOnlyList<VectorCandidate> candidates,
        decimal minSimilarity,
        int maxChunks,
        int contextBudget)
    {
        var queryTerms = TokenizeForLexicalScore(question);
        var ranked = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                RawScore = candidate.Score,
                AdjustedScore = CalculateAdjustedScore(candidate, queryTerms)
            })
            .Where(item => item.AdjustedScore >= minSimilarity)
            .GroupBy(item => DedupeKey(item.Candidate))
            .Select(group => group
                .OrderByDescending(item => item.AdjustedScore)
                .ThenBy(item => item.Candidate.ChunkIndex)
                .First())
            .OrderByDescending(item => item.AdjustedScore)
            .ThenBy(item => item.Candidate.CourseTitle)
            .ThenBy(item => item.Candidate.ChunkIndex)
            .ToList();

        var citations = new List<AiChatCitation>();
        var usedCharacters = 0;
        foreach (var item in ranked)
        {
            var citation = ToCitation(item.Candidate, item.AdjustedScore, item.RawScore);
            var nextLength = usedCharacters + citation.Snippet.Length;
            if (citations.Count > 0 && nextLength > contextBudget)
                break;

            citations.Add(citation);
            usedCharacters = nextLength;
            if (citations.Count >= maxChunks)
                break;
        }

        return citations;
    }

    internal static decimal CalculateAdjustedScore(VectorCandidate candidate, IReadOnlySet<string> queryTerms)
    {
        var lexicalScore = CalculateLexicalScore(candidate, queryTerms);
        return Math.Round(Math.Clamp(candidate.Score + lexicalScore, -1m, 1m), 4);
    }

    internal static decimal CalculateLexicalScore(VectorCandidate candidate, IReadOnlySet<string> queryTerms)
    {
        if (queryTerms.Count == 0)
            return 0m;

        var sourceText = string.Join(' ',
            candidate.CourseTitle,
            candidate.SectionTitle,
            candidate.LessonTitle,
            candidate.Text);
        var sourceTerms = TokenizeForLexicalScore(sourceText);
        var matches = queryTerms.Count(term => sourceTerms.Contains(term));
        if (matches == 0)
            return 0m;

        var ratio = (decimal)matches / queryTerms.Count;
        return Math.Min(0.12m, ratio * 0.12m);
    }

    internal static IReadOnlySet<string> TokenizeForLexicalScore(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in WordRegex().Matches(value.ToLowerInvariant()))
        {
            var token = match.Value;
            if (token.Length >= 3)
                terms.Add(NormalizeToken(token));
        }

        return terms;
    }

    private static string DedupeKey(VectorCandidate candidate)
    {
        if (candidate.LessonId.HasValue)
            return $"lesson:{candidate.LessonId.Value:N}";
        if (candidate.SectionId.HasValue)
            return $"section:{candidate.SectionId.Value:N}:{candidate.SourceType}";

        return $"chunk:{candidate.ChunkId:N}";
    }

    private static string NormalizeToken(string token)
    {
        if (token.EndsWith("ing", StringComparison.OrdinalIgnoreCase) && token.Length > 5)
            return token[..^3];
        if (token.EndsWith("ed", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            return token[..^2];
        if (token.EndsWith("s", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            return token[..^1];
        return token;
    }

    private static AiChatCitation ToCitation(VectorCandidate candidate, decimal adjustedScore, decimal rawScore) =>
        new(
            candidate.ChunkId,
            candidate.CourseId,
            candidate.SectionId,
            candidate.LessonId,
            candidate.CourseTitle,
            candidate.SectionTitle,
            candidate.LessonTitle,
            TrimSnippet(candidate.Text),
            adjustedScore,
            rawScore);

    private static string TrimSnippet(string text)
    {
        var normalized = text.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 420 ? normalized : normalized[..420].TrimEnd() + "...";
    }

    [GeneratedRegex("[\\p{L}\\p{N}]+", RegexOptions.Compiled)]
    private static partial Regex WordRegex();

    internal sealed record VectorCandidate(
        Guid ChunkId,
        Guid CourseId,
        Guid? SectionId,
        Guid? LessonId,
        string CourseTitle,
        string? SectionTitle,
        string? LessonTitle,
        string SourceType,
        int ChunkIndex,
        string Text,
        decimal Score);
}
