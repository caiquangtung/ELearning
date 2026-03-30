using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.UploadAsset;

public sealed class UploadLessonAssetCommandHandler(
    ICourseRepository courseRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadLessonAssetCommand, Result<ContentAssetDto>>
{
    public async Task<Result<ContentAssetDto>> Handle(UploadLessonAssetCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<ContentAssetDto>(Error.NotFound("Course", request.CourseId));

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        if (section is null)
            return Result.Failure<ContentAssetDto>(Error.NotFound("Section", request.SectionId));

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        if (lesson is null)
            return Result.Failure<ContentAssetDto>(Error.NotFound("Lesson", request.LessonId));

        FileStorageResult stored;
        try
        {
            stored = await fileStorage.SaveAsync(request.Content, request.FileName, request.ContentType, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<ContentAssetDto>(Error.Validation("Asset", ex.Message));
        }

        try
        {
            var asset = lesson.AddAsset(
                request.AssetType,
                stored.FileName,
                stored.ContentType,
                stored.SizeBytes,
                stored.StorageKey,
                stored.Url);

            courseRepository.Update(course);
            await unitOfWork.SaveChangesAsync(ct);

            return new ContentAssetDto(
                asset.Id,
                asset.AssetType,
                asset.FileName,
                asset.ContentType,
                asset.SizeBytes,
                asset.Url,
                asset.UploadedAt);
        }
        catch (DomainException ex)
        {
            return Result.Failure<ContentAssetDto>(Error.Validation("Asset", ex.Message));
        }
    }
}

