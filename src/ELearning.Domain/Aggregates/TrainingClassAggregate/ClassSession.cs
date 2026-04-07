using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.TrainingClassAggregate;

public sealed class ClassSession : Entity
{
    private ClassSession() { }

    public Guid TrainingClassId { get; private set; }
    public string Title { get; private set; } = default!;
    public ClassSessionType SessionType { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string? Location { get; private set; }
    public string? ZoomMeetingId { get; private set; }
    public string? ZoomJoinUrl { get; private set; }
    public ClassSessionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    internal static ClassSession Create(
        Guid trainingClassId,
        string title,
        ClassSessionType sessionType,
        DateTime startUtc,
        DateTime endUtc,
        string? location,
        string? zoomMeetingId,
        string? zoomJoinUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Session title is required.");
        if (endUtc <= startUtc)
            throw new DomainException("Session end must be after start.");

        return new ClassSession
        {
            Id = Guid.NewGuid(),
            TrainingClassId = trainingClassId,
            Title = title.Trim(),
            SessionType = sessionType,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            ZoomMeetingId = zoomMeetingId,
            ZoomJoinUrl = zoomJoinUrl,
            Status = ClassSessionStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateSchedule(
        string title,
        ClassSessionType sessionType,
        DateTime startUtc,
        DateTime endUtc,
        string? location,
        string? zoomMeetingId,
        string? zoomJoinUrl)
    {
        if (Status == ClassSessionStatus.Cancelled)
            throw new DomainException("Cannot update a cancelled session.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Session title is required.");
        if (endUtc <= startUtc)
            throw new DomainException("Session end must be after start.");

        Title = title.Trim();
        SessionType = sessionType;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        ZoomMeetingId = zoomMeetingId;
        ZoomJoinUrl = zoomJoinUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ClassSessionStatus.Cancelled) return;
        Status = ClassSessionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
