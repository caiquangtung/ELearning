using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.GetLessonVideo;

public sealed class GetLessonVideoQueryHandler(IVideoAssetRepository videoAssetRepository)
    : IRequestHandler<GetLessonVideoQuery, Result<VideoAssetDto>>
{
    public async Task<Result<VideoAssetDto>> Handle(GetLessonVideoQuery request, CancellationToken ct)
    {
        var video = await videoAssetRepository.GetByLessonAsync(request.LessonId, ct);
        return video is null
            ? Result.Failure<VideoAssetDto>(Error.NotFound("Video", request.LessonId))
            : VideoMapper.ToDto(video);
    }
}
