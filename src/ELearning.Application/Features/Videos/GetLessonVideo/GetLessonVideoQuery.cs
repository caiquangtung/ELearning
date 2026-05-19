using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.GetLessonVideo;

public sealed record GetLessonVideoQuery(Guid LessonId) : IRequest<Result<VideoAssetDto>>;
