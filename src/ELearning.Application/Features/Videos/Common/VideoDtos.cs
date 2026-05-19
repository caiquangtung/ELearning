namespace ELearning.Application.Features.Videos.Common;

public sealed record VideoAssetDto(
    Guid Id,
    Guid LessonId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url,
    int? DurationSeconds,
    DateTime UploadedAt);

public sealed record VideoPlaybackDto(
    Guid Id,
    Guid LessonId,
    string Url,
    string ContentType,
    int? DurationSeconds);

public sealed record WatchProgressDto(
    Guid Id,
    Guid VideoAssetId,
    Guid LessonId,
    Guid UserId,
    int LastPositionSeconds,
    int DurationSeconds,
    int WatchedSeconds,
    decimal ProgressPercent,
    bool IsCompleted,
    DateTime? CompletedAt);
