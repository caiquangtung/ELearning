using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Videos.UploadVideo;

public sealed record UploadVideoCommand(
    Guid CourseId,
    Guid SectionId,
    Guid LessonId,
    Stream Content,
    string FileName,
    string ContentType,
    int? DurationSeconds) : IRequest<Result<VideoAssetDto>>;
