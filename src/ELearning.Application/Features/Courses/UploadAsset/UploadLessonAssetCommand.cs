using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using MediatR;

namespace ELearning.Application.Features.Courses.UploadAsset;

public sealed record UploadLessonAssetCommand(
    Guid CourseId,
    Guid SectionId,
    Guid LessonId,
    ContentAssetType AssetType,
    Stream Content,
    string FileName,
    string ContentType)
    : IRequest<Result<ContentAssetDto>>;

