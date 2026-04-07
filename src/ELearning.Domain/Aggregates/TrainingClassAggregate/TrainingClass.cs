using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.TrainingClassAggregate;

public sealed class TrainingClass : AggregateRoot
{
    private TrainingClass() { }

    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = default!;
    public int MaxLearners { get; private set; }
    public TrainingClassStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public List<ClassInstructor> Instructors { get; private set; } = [];
    public List<ClassSession> Sessions { get; private set; } = [];

    public static TrainingClass Create(Guid courseId, string title, int maxLearners)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Class title is required.");
        if (maxLearners < 1)
            throw new DomainException("Max learners must be at least 1.");

        return new TrainingClass
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title.Trim(),
            MaxLearners = maxLearners,
            Status = TrainingClassStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, int maxLearners)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Class title is required.");
        if (maxLearners < 1)
            throw new DomainException("Max learners must be at least 1.");

        Title = title.Trim();
        MaxLearners = maxLearners;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddInstructor(Guid userId)
    {
        if (Instructors.Any(i => i.UserId == userId))
            return;

        Instructors.Add(ClassInstructor.Create(Id, userId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveInstructor(Guid userId)
    {
        var row = Instructors.FirstOrDefault(i => i.UserId == userId);
        if (row is null)
            throw new DomainException("Instructor is not assigned to this class.");

        Instructors.Remove(row);
        UpdatedAt = DateTime.UtcNow;
    }

    public ClassSession ScheduleSession(
        string title,
        ClassSessionType sessionType,
        DateTime startUtc,
        DateTime endUtc,
        string? location,
        string? zoomMeetingId,
        string? zoomJoinUrl)
    {
        if (Status == TrainingClassStatus.Cancelled)
            throw new DomainException("Cannot schedule sessions for a cancelled class.");
        if (Instructors.Count == 0)
            throw new DomainException("At least one instructor must be assigned before scheduling sessions.");

        var session = ClassSession.Create(
            Id,
            title,
            sessionType,
            startUtc,
            endUtc,
            location,
            zoomMeetingId,
            zoomJoinUrl);

        Sessions.Add(session);

        if (Status == TrainingClassStatus.Draft)
            Status = TrainingClassStatus.Scheduled;

        UpdatedAt = DateTime.UtcNow;
        return session;
    }

    public void MarkCancelled()
    {
        if (Status == TrainingClassStatus.Cancelled) return;
        Status = TrainingClassStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
