using ELearning.Application.Features.Videos.TrackWatchProgress;
using ELearning.Application.Features.Videos.UploadVideo;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class VideosFeatureSmokeTests
{
    [Fact]
    public void UploadVideoValidator_rejects_non_video_content_type()
    {
        var validator = new UploadVideoCommandValidator();

        var result = validator.Validate(new UploadVideoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Stream.Null,
            "lesson.pdf",
            "application/pdf",
            null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TrackWatchProgressValidator_requires_positive_duration()
    {
        var validator = new TrackWatchProgressCommandValidator();

        var result = validator.Validate(new TrackWatchProgressCommand(
            Guid.NewGuid(),
            PositionSeconds: 0,
            DurationSeconds: 0,
            WatchedSeconds: 0));

        result.IsValid.Should().BeFalse();
    }
}
