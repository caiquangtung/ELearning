using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiKnowledgeChunk : AuditableAggregateRoot
{
    private AiKnowledgeChunk() { }

    public Guid CourseId { get; private set; }
    public Guid? SectionId { get; private set; }
    public Guid? LessonId { get; private set; }
    public string SourceType { get; private set; } = default!;
    public string CourseTitle { get; private set; } = default!;
    public string? SectionTitle { get; private set; }
    public string? LessonTitle { get; private set; }
    public int ChunkIndex { get; private set; }
    public string ContentHash { get; private set; } = default!;
    public string Text { get; private set; } = default!;
    public string EmbeddingJson { get; private set; } = default!;
    public string MetadataJson { get; private set; } = "{}";

    public static AiKnowledgeChunk Create(
        Guid courseId,
        Guid? sectionId,
        Guid? lessonId,
        string sourceType,
        string courseTitle,
        string? sectionTitle,
        string? lessonTitle,
        int chunkIndex,
        string contentHash,
        string text,
        string embeddingJson,
        string metadataJson)
    {
        if (courseId == Guid.Empty)
            throw new DomainException("Knowledge chunk course is required.");
        if (string.IsNullOrWhiteSpace(sourceType))
            throw new DomainException("Knowledge chunk source type is required.");
        if (string.IsNullOrWhiteSpace(courseTitle))
            throw new DomainException("Knowledge chunk course title is required.");
        if (chunkIndex < 0)
            throw new DomainException("Knowledge chunk index must be non-negative.");
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new DomainException("Knowledge chunk content hash is required.");
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Knowledge chunk text is required.");
        if (string.IsNullOrWhiteSpace(embeddingJson))
            throw new DomainException("Knowledge chunk embedding is required.");

        return new AiKnowledgeChunk
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            SectionId = sectionId,
            LessonId = lessonId,
            SourceType = sourceType.Trim(),
            CourseTitle = courseTitle.Trim(),
            SectionTitle = string.IsNullOrWhiteSpace(sectionTitle) ? null : sectionTitle.Trim(),
            LessonTitle = string.IsNullOrWhiteSpace(lessonTitle) ? null : lessonTitle.Trim(),
            ChunkIndex = chunkIndex,
            ContentHash = contentHash.Trim(),
            Text = text.Trim(),
            EmbeddingJson = embeddingJson.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
