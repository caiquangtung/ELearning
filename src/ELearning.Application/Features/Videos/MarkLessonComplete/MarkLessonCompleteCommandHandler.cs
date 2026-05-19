using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.VideoAggregate;
using MediatR;

namespace ELearning.Application.Features.Videos.MarkLessonComplete;

public sealed class MarkLessonCompleteCommandHandler(
    IVideoAssetRepository videoAssetRepository,
    IWatchEventRepository watchEventRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarkLessonCompleteCommand, Result<WatchProgressDto>>
{
    public async Task<Result<WatchProgressDto>> Handle(MarkLessonCompleteCommand request, CancellationToken ct)
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

        progress.MarkCompleted(DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);

        return VideoMapper.ToDto(progress);
    }
}
