using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CourseAggregate;

public sealed class Section : Entity
{
    private Section() { }

    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = default!;
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public List<Lesson> Lessons { get; private set; } = [];

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    internal static Section Create(Guid courseId, string title, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Section title is required.");

        return new Section
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title.Trim(),
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Section title is required.");
        Title = title.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public Lesson AddLesson(string title)
    {
        var sortOrder = Lessons.Count == 0 ? 1 : Lessons.Max(l => l.SortOrder) + 1;
        var lesson = Lesson.Create(Id, title, sortOrder);
        Lessons.Add(lesson);
        UpdatedAt = DateTime.UtcNow;
        return lesson;
    }
}

