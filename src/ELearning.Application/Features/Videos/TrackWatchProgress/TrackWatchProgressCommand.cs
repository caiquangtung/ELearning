using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.TrackWatchProgress;

public sealed record TrackWatchProgressCommand(
    Guid VideoAssetId,
    int PositionSeconds,
    int DurationSeconds,
    int WatchedSeconds) : IRequest<Result<WatchProgressDto>>;
