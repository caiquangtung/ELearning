namespace ELearning.Core.Abstractions;

public sealed record ZoomMeetingCreateRequest(
    string Topic,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Timezone = "UTC");

public sealed record ZoomMeetingInfo(
    string MeetingId,
    string JoinUrl,
    string? Password);

public interface IZoomMeetingService
{
    Task<ZoomMeetingInfo> CreateMeetingAsync(
        ZoomMeetingCreateRequest request,
        CancellationToken cancellationToken = default);
}
