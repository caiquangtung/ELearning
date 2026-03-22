namespace ELearning.Application.Common.Interfaces;

public record ZoomMeetingRequest(string Topic, DateTime StartTime, int DurationMinutes, string HostEmail);
public record ZoomMeetingResult(string MeetingId, string JoinUrl, string HostUrl, string Password);
public record ZoomParticipant(string Email, string Name, DateTime JoinTime, DateTime LeaveTime);

public interface IZoomService
{
    Task<ZoomMeetingResult> CreateMeetingAsync(ZoomMeetingRequest request, CancellationToken ct = default);
    Task DeleteMeetingAsync(string meetingId, CancellationToken ct = default);
    Task<IReadOnlyList<ZoomParticipant>> GetParticipantsAsync(string meetingId, CancellationToken ct = default);
}
