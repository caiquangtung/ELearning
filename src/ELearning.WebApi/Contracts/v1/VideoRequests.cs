namespace ELearning.WebApi.Contracts.v1;

public sealed class UploadVideoRequest
{
    public IFormFile File { get; set; } = default!;
    public int? DurationSeconds { get; set; }
}

public sealed record TrackWatchProgressRequest(
    int PositionSeconds,
    int DurationSeconds,
    int WatchedSeconds);
