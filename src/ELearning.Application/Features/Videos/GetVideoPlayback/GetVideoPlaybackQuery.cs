using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.GetVideoPlayback;

public sealed record GetVideoPlaybackQuery(Guid VideoAssetId) : IRequest<Result<VideoPlaybackDto>>;
