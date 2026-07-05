using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeIndexingService(
    ApplicationDbContext context,
    AiKnowledgeChunker chunker,
    IAiTextEmbeddingService embeddingService)
    : IAiKnowledgeIndexingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiKnowledgeReindexResult> ReindexAsync(
        Guid? courseId,
        Guid? requestedByUserId = null,
        Guid? jobId = null,
        CancellationToken ct = default)
    {
        var job = await StartJobAsync(courseId, requestedByUserId, jobId, ct);

        try
        {
            var courses = await context.Courses
                .AsSplitQuery()
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
                .Where(c => !courseId.HasValue || c.Id == courseId.Value)
                .OrderBy(c => c.Title)
                .ToListAsync(ct);

            var scopeCourseIds = courseId.HasValue
                ? [courseId.Value]
                : await context.Courses.IgnoreQueryFilters().Select(c => c.Id).ToListAsync(ct);

            var existing = await context.AiKnowledgeChunks
                .Where(x => scopeCourseIds.Contains(x.CourseId))
                .ToListAsync(ct);

            var existingByHash = existing.ToDictionary(x => x.ContentHash, StringComparer.OrdinalIgnoreCase);
            var desired = new List<IndexedChunk>();
            foreach (var source in courses.SelectMany(course => chunker.BuildCourseChunks(course)))
                desired.Add(await CreateIndexedChunkAsync(source, ct));

            var desiredHashes = desired.Select(x => x.ContentHash).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var stale = existing.Where(x => !desiredHashes.Contains(x.ContentHash)).ToList();
            context.AiKnowledgeChunks.RemoveRange(stale);

            var vectorUpdates = new List<VectorUpdate>(desired.Count);
            var newChunks = new List<AiKnowledgeChunk>();

            foreach (var desiredChunk in desired)
            {
                if (existingByHash.TryGetValue(desiredChunk.ContentHash, out var existingChunk))
                {
                    existingChunk.UpdateEmbedding(desiredChunk.EmbeddingJson, desiredChunk.MetadataJson);
                    vectorUpdates.Add(new VectorUpdate(existingChunk.Id, desiredChunk.VectorLiteral));
                    continue;
                }

                var newChunk = CreateChunk(desiredChunk);
                newChunks.Add(newChunk);
                vectorUpdates.Add(new VectorUpdate(newChunk.Id, desiredChunk.VectorLiteral));
            }

            await context.AiKnowledgeChunks.AddRangeAsync(newChunks, ct);
            await context.SaveChangesAsync(ct);
            await UpdateEmbeddingVectorsAsync(vectorUpdates, ct);

            job.MarkSucceeded(courses.Count, desired.Count, stale.Count);
            await context.SaveChangesAsync(ct);

            return new AiKnowledgeReindexResult(job.Id, courses.Count, desired.Count, stale.Count);
        }
        catch (Exception ex)
        {
            await MarkJobFailedAsync(job.Id, ex.Message, ct);
            throw;
        }
    }

    public async Task<AiKnowledgeStatusResult> GetStatusAsync(CancellationToken ct = default)
    {
        var totalChunks = await context.AiKnowledgeChunks.CountAsync(ct);
        var indexedCourses = await context.AiKnowledgeChunks
            .Select(x => x.CourseId)
            .Distinct()
            .CountAsync(ct);
        var queuedJobs = await context.AiKnowledgeReindexJobs
            .CountAsync(x => x.Status == AiKnowledgeReindexJobStatus.Queued, ct);
        var inProgressJobs = await context.AiKnowledgeReindexJobs
            .CountAsync(x => x.Status == AiKnowledgeReindexJobStatus.InProgress, ct);
        var failedJobs = await context.AiKnowledgeReindexJobs
            .CountAsync(x => x.Status == AiKnowledgeReindexJobStatus.Failed, ct);
        var failedAiRequests = await context.AiRequestLogs
            .CountAsync(x => x.Status == AiRequestStatus.Failed, ct);
        var vectorizedChunks = await CountVectorizedChunksAsync(ct);

        var jobs = await context.AiKnowledgeReindexJobs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(ct);
        var evaluations = await context.AiRagEvaluationRuns
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var modelProbe = await embeddingService.EmbedAsync(
            new AiTextEmbeddingRequest("status", AiTextEmbeddingPurpose.StatusProbe),
            ct);
        var recentJobs = jobs.Select(ToJobSummary).ToList();
        var recentEvaluations = evaluations.Select(ToEvaluationSummary).ToList();

        return new AiKnowledgeStatusResult(
            totalChunks,
            vectorizedChunks,
            indexedCourses,
            queuedJobs,
            inProgressJobs,
            failedJobs,
            failedAiRequests,
            modelProbe.Dimensions,
            modelProbe.Provider,
            modelProbe.Model,
            recentJobs.FirstOrDefault(),
            recentJobs,
            recentEvaluations.FirstOrDefault(),
            recentEvaluations);
    }

    private async Task<IndexedChunk> CreateIndexedChunkAsync(
        AiKnowledgeChunkSource source,
        CancellationToken ct)
    {
        var embedding = await embeddingService.EmbedAsync(
            new AiTextEmbeddingRequest(
                source.Text,
                AiTextEmbeddingPurpose.RetrievalDocument,
                BuildEmbeddingTitle(source)),
            ct);
        var embeddingJson = PgVectorFormatter.ToJson(embedding.Vector);
        var vectorLiteral = PgVectorFormatter.ToVectorLiteral(embedding.Vector);
        var metadataJson = JsonSerializer.Serialize(new
        {
            source.SourceType,
            source.CourseTitle,
            source.SectionTitle,
            source.LessonTitle,
            embedding.Provider,
            embedding.Model,
            embedding.Dimensions
        }, JsonOptions);

        return new IndexedChunk(
            source,
            ComputeContentHash(source),
            embeddingJson,
            vectorLiteral,
            metadataJson);
    }

    private static AiKnowledgeChunk CreateChunk(IndexedChunk chunk)
    {
        var source = chunk.Source;
        return AiKnowledgeChunk.Create(
            source.CourseId,
            source.SectionId,
            source.LessonId,
            source.SourceType,
            source.CourseTitle,
            source.SectionTitle,
            source.LessonTitle,
            source.ChunkIndex,
            chunk.ContentHash,
            source.Text,
            chunk.EmbeddingJson,
            chunk.MetadataJson);
    }

    private static string BuildEmbeddingTitle(AiKnowledgeChunkSource source) =>
        string.Join(
            " - ",
            new[] { source.CourseTitle, source.SectionTitle, source.LessonTitle }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string ComputeContentHash(AiKnowledgeChunkSource source)
    {
        var raw = string.Join('|',
            source.CourseId,
            source.SectionId,
            source.LessonId,
            source.SourceType,
            source.ChunkIndex,
            source.CourseTitle,
            source.SectionTitle,
            source.LessonTitle,
            source.Text);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task<AiKnowledgeReindexJob> StartJobAsync(
        Guid? courseId,
        Guid? requestedByUserId,
        Guid? jobId,
        CancellationToken ct)
    {
        AiKnowledgeReindexJob job;
        if (jobId.HasValue)
        {
            job = await context.AiKnowledgeReindexJobs.FirstOrDefaultAsync(x => x.Id == jobId.Value, ct)
                ?? throw new InvalidOperationException("AI knowledge reindex job was not found.");
        }
        else
        {
            job = AiKnowledgeReindexJob.Create(courseId, requestedByUserId);
            await context.AiKnowledgeReindexJobs.AddAsync(job, ct);
        }

        job.MarkInProgress();
        await context.SaveChangesAsync(ct);
        return job;
    }

    private async Task MarkJobFailedAsync(Guid jobId, string error, CancellationToken ct)
    {
        context.ChangeTracker.Clear();
        var job = await context.AiKnowledgeReindexJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job is null)
            return;

        job.MarkFailed(error);
        await context.SaveChangesAsync(ct);
    }

    private async Task UpdateEmbeddingVectorsAsync(IReadOnlyList<VectorUpdate> updates, CancellationToken ct)
    {
        if (updates.Count == 0)
            return;

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            foreach (var update in updates)
            {
                await using var command = connection.CreateCommand();
                command.CommandText =
                    """
                    UPDATE ai_knowledge_chunks
                    SET embedding_vector = CAST(@embedding_vector AS vector(768))
                    WHERE id = @id
                    """;
                command.Parameters.Add(new NpgsqlParameter("embedding_vector", update.VectorLiteral));
                command.Parameters.Add(new NpgsqlParameter("id", update.ChunkId));
                await command.ExecuteNonQueryAsync(ct);
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private async Task<int> CountVectorizedChunksAsync(CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ai_knowledge_chunks WHERE embedding_vector IS NOT NULL";
            var value = await command.ExecuteScalarAsync(ct);
            return Convert.ToInt32(value);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static AiKnowledgeReindexJobSummary ToJobSummary(AiKnowledgeReindexJob job) =>
        new(
            job.Id,
            job.CourseId,
            job.Status.ToString(),
            job.RequestedByUserId,
            job.StartedAt,
            job.CompletedAt,
            job.IndexedCourses,
            job.IndexedChunks,
            job.DeletedStaleChunks,
            job.Error,
            job.CreatedAt);

    private static AiRagEvaluationRunSummary ToEvaluationSummary(AiRagEvaluationRun run) =>
        new(
            run.Id,
            run.Status.ToString(),
            run.RequestedByUserId,
            run.DatasetVersion,
            run.TotalCases,
            run.PassedCases,
            run.RetrievalHitRate,
            run.CitationValidityRate,
            run.RefusalAccuracyRate,
            run.GroundednessRate,
            run.Error,
            run.StartedAt,
            run.CompletedAt,
            run.CreatedAt);

    private sealed record IndexedChunk(
        AiKnowledgeChunkSource Source,
        string ContentHash,
        string EmbeddingJson,
        string VectorLiteral,
        string MetadataJson);

    private sealed record VectorUpdate(Guid ChunkId, string VectorLiteral);
}
