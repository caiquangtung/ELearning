using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.MarkLessonComplete;

public sealed record MarkLessonCompleteCommand(Guid VideoAssetId) : IRequest<Result<WatchProgressDto>>;
