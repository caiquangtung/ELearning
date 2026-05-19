using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.GetVideoPlayback;

public sealed class GetVideoPlaybackQueryHandler(IVideoAssetRepository videoAssetRepository)
    : IRequestHandler<GetVideoPlaybackQuery, Result<VideoPlaybackDto>>
{
    public async Task<Result<VideoPlaybackDto>> Handle(GetVideoPlaybackQuery request, CancellationToken ct)
    {
        var video = await videoAssetRepository.GetByIdAsync(request.VideoAssetId, ct);
        return video is null
            ? Result.Failure<VideoPlaybackDto>(Error.NotFound("Video", request.VideoAssetId))
            : VideoMapper.ToPlaybackDto(video);
    }
}
