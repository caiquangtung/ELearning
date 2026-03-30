using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CourseAggregate;

public sealed class Course : AggregateRoot
{
    private Course() { }

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public CourseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public List<Section> Sections { get; private set; } = [];

    public static Course Create(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Course title is required.");

        return new Course
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = CourseStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Course title is required.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public Section AddSection(string title)
    {
        var sortOrder = Sections.Count == 0 ? 1 : Sections.Max(s => s.SortOrder) + 1;
        var section = Section.Create(Id, title, sortOrder);
        Sections.Add(section);
        UpdatedAt = DateTime.UtcNow;
        return section;
    }

    public void Publish()
    {
        if (Status == CourseStatus.Published) return;
        if (Sections.Count == 0 || Sections.All(s => s.Lessons.Count == 0))
            throw new DomainException("Course must have at least one lesson before publishing.");

        Status = CourseStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        if (Status == CourseStatus.Draft) return;
        Status = CourseStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }
}

