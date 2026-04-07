namespace ELearning.Core.Abstractions;

public sealed record ZoomMeetingInfo(string MeetingId, string JoinUrl, string? Password);

public interface IZoomMeetingService
{
    Task<ZoomMeetingInfo> CreateMeetingAsync(
        string topic,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);
}
