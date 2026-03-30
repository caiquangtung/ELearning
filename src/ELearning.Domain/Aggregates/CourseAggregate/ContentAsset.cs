using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CourseAggregate;

public sealed class ContentAsset : Entity
{
    private ContentAsset() { }

    public Guid LessonId { get; private set; }
    public ContentAssetType AssetType { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; } = default!;
    public string Url { get; private set; } = default!;
    public DateTime UploadedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    internal static ContentAsset Create(
        Guid lessonId,
        ContentAssetType assetType,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        string url)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name is required.");
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Asset URL is required.");
        if (sizeBytes <= 0)
            throw new DomainException("Asset size must be greater than 0.");

        return new ContentAsset
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            AssetType = assetType,
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            SizeBytes = sizeBytes,
            StorageKey = storageKey.Trim(),
            Url = url.Trim(),
            UploadedAt = DateTime.UtcNow
        };
    }
}

