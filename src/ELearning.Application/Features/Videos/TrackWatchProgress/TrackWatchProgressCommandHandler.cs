using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Videos.TrackWatchProgress;

public sealed class TrackWatchProgressCommandHandler(
    IVideoAssetRepository videoAssetRepository,
    IWatchEventRepository watchEventRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<TrackWatchProgressCommand, Result<WatchProgressDto>>
{
    public async Task<Result<WatchProgressDto>> Handle(TrackWatchProgressCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<WatchProgressDto>(Error.Unauthorized());

        var video = await videoAssetRepository.GetByIdAsync(request.VideoAssetId, ct);
        if (video is null)
            return Result.Failure<WatchProgressDto>(Error.NotFound("Video", request.VideoAssetId));

        var progress = await watchEventRepository.GetForUserAsync(video.Id, userId.Value, ct);
        if (progress is null)
        {
            progress = WatchEvent.Start(video.Id, video.LessonId, userId.Value);
            watchEventRepository.Add(progress);
        }

        try
        {
            progress.RecordProgress(request.PositionSeconds, request.DurationSeconds, request.WatchedSeconds, DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(ct);
            return VideoMapper.ToDto(progress);
        }
        catch (DomainException ex)
        {
            return Result.Failure<WatchProgressDto>(Error.Validation("WatchProgress", ex.Message));
        }
    }
}
