using ELearning.Core.Abstractions;

namespace ELearning.Infrastructure.Zoom;

/// <summary>
/// Placeholder Zoom integration for local/dev. Replace with a real implementation that calls Zoom APIs.
/// </summary>
public sealed class NoOpZoomMeetingService : IZoomMeetingService
{
    public Task<ZoomMeetingInfo> CreateMeetingAsync(
        string topic,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        return Task.FromResult(
            new ZoomMeetingInfo(
                MeetingId: $"zoom-{suffix}",
                JoinUrl: $"https://zoom.example.invalid/j/{suffix}",
                Password: null));
    }
}
