using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeIndexingService(
    ApplicationDbContext context,
    AiKnowledgeChunker chunker,
    IAiEmbeddingService embeddingService)
    : IAiKnowledgeIndexingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiKnowledgeReindexResult> ReindexAsync(Guid? courseId, CancellationToken ct = default)
    {
        var courses = await context.Courses
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

        var desired = courses
            .SelectMany(course => chunker.BuildCourseChunks(course))
            .Select(source => new IndexedChunk(source, ComputeContentHash(source)))
            .ToList();

        var desiredHashes = desired.Select(x => x.ContentHash).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var stale = existing.Where(x => !desiredHashes.Contains(x.ContentHash)).ToList();
        context.AiKnowledgeChunks.RemoveRange(stale);

        var existingHashes = existing.Select(x => x.ContentHash).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newChunks = desired
            .Where(x => !existingHashes.Contains(x.ContentHash))
            .Select(x => CreateChunk(x.Source, x.ContentHash))
            .ToList();

        await context.AiKnowledgeChunks.AddRangeAsync(newChunks, ct);
        await context.SaveChangesAsync(ct);

        return new AiKnowledgeReindexResult(courses.Count, desired.Count, stale.Count);
    }

    private AiKnowledgeChunk CreateChunk(AiKnowledgeChunkSource source, string contentHash)
    {
        var embedding = embeddingService.Embed(source.Text);
        var embeddingJson = JsonSerializer.Serialize(embedding, JsonOptions);
        var metadataJson = JsonSerializer.Serialize(new
        {
            source.SourceType,
            source.CourseTitle,
            source.SectionTitle,
            source.LessonTitle
        }, JsonOptions);

        return AiKnowledgeChunk.Create(
            source.CourseId,
            source.SectionId,
            source.LessonId,
            source.SourceType,
            source.CourseTitle,
            source.SectionTitle,
            source.LessonTitle,
            source.ChunkIndex,
            contentHash,
            source.Text,
            embeddingJson,
            metadataJson);
    }

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

    private sealed record IndexedChunk(AiKnowledgeChunkSource Source, string ContentHash);
}
