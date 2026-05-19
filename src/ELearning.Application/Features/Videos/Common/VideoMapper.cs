using ELearning.Domain.Aggregates.VideoAggregate;

namespace ELearning.Application.Features.Videos.Common;

public static class VideoMapper
{
    public static VideoAssetDto ToDto(VideoAsset video) => new(
        video.Id,
        video.LessonId,
        video.FileName,
        video.ContentType,
        video.SizeBytes,
        video.Url,
        video.DurationSeconds,
        video.UploadedAt);

    public static VideoPlaybackDto ToPlaybackDto(VideoAsset video) => new(
        video.Id,
        video.LessonId,
        video.Url,
        video.ContentType,
        video.DurationSeconds);

    public static WatchProgressDto ToDto(WatchEvent progress) => new(
        progress.Id,
        progress.VideoAssetId,
        progress.LessonId,
        progress.UserId,
        progress.LastPositionSeconds,
        progress.DurationSeconds,
        progress.WatchedSeconds,
        progress.ProgressPercent,
        progress.IsCompleted,
        progress.CompletedAt);
}
