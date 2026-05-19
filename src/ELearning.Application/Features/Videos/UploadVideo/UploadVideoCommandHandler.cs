using ELearning.Application.Features.Videos.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Videos.UploadVideo;

public sealed class UploadVideoCommandHandler(
    ICourseRepository courseRepository,
    IFileStorage fileStorage,
    IVideoAssetRepository videoAssetRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadVideoCommand, Result<VideoAssetDto>>
{
    public async Task<Result<VideoAssetDto>> Handle(UploadVideoCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<VideoAssetDto>(Error.NotFound("Course", request.CourseId));

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        if (section is null)
            return Result.Failure<VideoAssetDto>(Error.NotFound("Section", request.SectionId));

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        if (lesson is null)
            return Result.Failure<VideoAssetDto>(Error.NotFound("Lesson", request.LessonId));

        FileStorageResult stored;
        try
        {
            stored = await fileStorage.SaveAsync(request.Content, request.FileName, request.ContentType, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<VideoAssetDto>(Error.Validation("Video", ex.Message));
        }

        try
        {
            var video = VideoAsset.Create(
                lesson.Id,
                stored.FileName,
                stored.ContentType,
                stored.SizeBytes,
                stored.StorageKey,
                stored.Url,
                request.DurationSeconds);

            videoAssetRepository.Add(video);
            await unitOfWork.SaveChangesAsync(ct);

            return VideoMapper.ToDto(video);
        }
        catch (DomainException ex)
        {
            return Result.Failure<VideoAssetDto>(Error.Validation("Video", ex.Message));
        }
    }
}
