using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.VideoAggregate;

public sealed class WatchEvent : AuditableAggregateRoot
{
    private const decimal CompletionThreshold = 80m;

    private WatchEvent() { }

    public Guid VideoAssetId { get; private set; }
    public Guid LessonId { get; private set; }
    public Guid UserId { get; private set; }
    public int LastPositionSeconds { get; private set; }
    public int DurationSeconds { get; private set; }
    public int WatchedSeconds { get; private set; }
    public decimal ProgressPercent { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public static WatchEvent Start(Guid videoAssetId, Guid lessonId, Guid userId)
    {
        if (videoAssetId == Guid.Empty)
            throw new DomainException("Video is required.");
        if (lessonId == Guid.Empty)
            throw new DomainException("Lesson is required.");
        if (userId == Guid.Empty)
            throw new DomainException("User is required.");

        return new WatchEvent
        {
            Id = Guid.NewGuid(),
            VideoAssetId = videoAssetId,
            LessonId = lessonId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordProgress(int positionSeconds, int durationSeconds, int watchedSeconds, DateTime utcNow)
    {
        if (positionSeconds < 0)
            throw new DomainException("Position must be non-negative.");
        if (durationSeconds <= 0)
            throw new DomainException("Duration must be positive.");
        if (watchedSeconds < 0)
            throw new DomainException("Watched seconds must be non-negative.");

        DurationSeconds = durationSeconds;
        LastPositionSeconds = Math.Min(positionSeconds, durationSeconds);
        WatchedSeconds = Math.Min(Math.Max(WatchedSeconds, watchedSeconds), durationSeconds);
        ProgressPercent = Math.Round(WatchedSeconds * 100m / durationSeconds, 2);

        if (!IsCompleted && ProgressPercent >= CompletionThreshold)
        {
            IsCompleted = true;
            CompletedAt = utcNow;
        }

        UpdatedAt = utcNow;
    }

    public void MarkCompleted(DateTime utcNow)
    {
        if (IsCompleted)
            return;

        IsCompleted = true;
        CompletedAt = utcNow;
        ProgressPercent = 100m;
        if (DurationSeconds > 0)
        {
            WatchedSeconds = DurationSeconds;
            LastPositionSeconds = DurationSeconds;
        }
        UpdatedAt = utcNow;
    }
}
