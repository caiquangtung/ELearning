using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.VideoAggregate;

public sealed class VideoAsset : AuditableAggregateRoot
{
    private VideoAsset() { }

    public Guid LessonId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; } = default!;
    public string Url { get; private set; } = default!;
    public int? DurationSeconds { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public static VideoAsset Create(
        Guid lessonId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        string url,
        int? durationSeconds)
    {
        if (lessonId == Guid.Empty)
            throw new DomainException("Lesson is required.");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name is required.");
        if (string.IsNullOrWhiteSpace(contentType) || !contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("A video content type is required.");
        if (sizeBytes <= 0)
            throw new DomainException("Video size must be greater than 0.");
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Video URL is required.");
        if (durationSeconds is <= 0)
            throw new DomainException("Duration must be positive when provided.");

        var now = DateTime.UtcNow;
        return new VideoAsset
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            SizeBytes = sizeBytes,
            StorageKey = storageKey.Trim(),
            Url = url.Trim(),
            DurationSeconds = durationSeconds,
            UploadedAt = now,
            CreatedAt = now
        };
    }
}
