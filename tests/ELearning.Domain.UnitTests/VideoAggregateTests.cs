using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class VideoAggregateTests
{
    [Fact]
    public void Create_video_accepts_video_content_type()
    {
        var video = VideoAsset.Create(
            Guid.NewGuid(),
            "lesson.mp4",
            "video/mp4",
            1024,
            "storage-key.mp4",
            "/api/v1/assets/storage-key.mp4",
            120);

        video.FileName.Should().Be("lesson.mp4");
        video.ContentType.Should().Be("video/mp4");
        video.DurationSeconds.Should().Be(120);
    }

    [Fact]
    public void Create_video_rejects_non_video_content_type()
    {
        var act = () => VideoAsset.Create(
            Guid.NewGuid(),
            "lesson.pdf",
            "application/pdf",
            1024,
            "storage-key.pdf",
            "/api/v1/assets/storage-key.pdf",
            null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Record_progress_marks_completed_at_eighty_percent()
    {
        var progress = WatchEvent.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        progress.RecordProgress(80, 100, 80, DateTime.UtcNow);

        progress.ProgressPercent.Should().Be(80m);
        progress.IsCompleted.Should().BeTrue();
        progress.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Mark_completed_is_idempotent()
    {
        var progress = WatchEvent.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var firstCompletedAt = DateTime.UtcNow;

        progress.MarkCompleted(firstCompletedAt);
        progress.MarkCompleted(firstCompletedAt.AddMinutes(5));

        progress.IsCompleted.Should().BeTrue();
        progress.CompletedAt.Should().Be(firstCompletedAt);
    }
}
