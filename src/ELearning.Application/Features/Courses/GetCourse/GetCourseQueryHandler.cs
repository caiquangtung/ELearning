using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.GetCourse;

public sealed class GetCourseQueryHandler(ICourseRepository courseRepository)
    : IRequestHandler<GetCourseQuery, Result<CourseDetailDto>>
{
    public async Task<Result<CourseDetailDto>> Handle(GetCourseQuery request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.Id, ct);
        if (course is null)
            return Result.Failure<CourseDetailDto>(Error.NotFound("Course", request.Id));

        var dto = new CourseDetailDto(
            course.Id,
            course.Title,
            course.Description,
            course.Status.ToString(),
            course.CreatedAt,
            course.UpdatedAt,
            course.Sections
                .OrderBy(s => s.SortOrder)
                .Select(s => new CourseSectionDetailDto(
                    s.Id,
                    s.Title,
                    s.SortOrder,
                    s.Lessons
                        .OrderBy(l => l.SortOrder)
                        .Select(l => new CourseLessonDetailDto(
                            l.Id,
                            l.Title,
                            l.SortOrder,
                            l.Content,
                            l.Assets
                                .OrderByDescending(a => a.UploadedAt)
                                .Select(a => new ContentAssetDto(
                                    a.Id,
                                    a.AssetType,
                                    a.FileName,
                                    a.ContentType,
                                    a.SizeBytes,
                                    a.Url,
                                    a.UploadedAt))
                                .ToList()))
                        .ToList()))
                .ToList());

        return dto;
    }
}

