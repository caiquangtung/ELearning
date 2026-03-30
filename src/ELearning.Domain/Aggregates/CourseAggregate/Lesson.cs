using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CourseAggregate;

public sealed class Lesson : Entity
{
    private Lesson() { }

    public Guid SectionId { get; private set; }
    public string Title { get; private set; } = default!;
    public int SortOrder { get; private set; }
    public string? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public List<ContentAsset> Assets { get; private set; } = [];

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    internal static Lesson Create(Guid sectionId, string title, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Lesson title is required.");

        return new Lesson
        {
            Id = Guid.NewGuid(),
            SectionId = sectionId,
            Title = title.Trim(),
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Lesson title is required.");
        Title = title.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string? content)
    {
        Content = string.IsNullOrWhiteSpace(content) ? null : content;
        UpdatedAt = DateTime.UtcNow;
    }

    public ContentAsset AddAsset(
        ContentAssetType assetType,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        string url)
    {
        var asset = ContentAsset.Create(Id, assetType, fileName, contentType, sizeBytes, storageKey, url);
        Assets.Add(asset);
        UpdatedAt = DateTime.UtcNow;
        return asset;
    }
}

