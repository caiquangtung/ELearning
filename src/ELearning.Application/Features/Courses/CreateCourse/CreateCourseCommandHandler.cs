using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using MediatR;

namespace ELearning.Application.Features.Courses.CreateCourse;

public sealed class CreateCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
{
    public async Task<Result<CourseDto>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        var course = Course.Create(request.Title, request.Description);
        courseRepository.Add(course);
        await unitOfWork.SaveChangesAsync(ct);

        return new CourseDto(
            course.Id,
            course.Title,
            course.Description,
            course.Status.ToString(),
            course.CreatedAt,
            course.UpdatedAt);
    }
}

