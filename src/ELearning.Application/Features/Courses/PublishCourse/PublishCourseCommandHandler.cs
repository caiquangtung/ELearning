using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.PublishCourse;

public sealed class PublishCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PublishCourseCommand, Result>
{
    public async Task<Result> Handle(PublishCourseCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.Id, ct);
        if (course is null)
            return Result.Failure(Error.NotFound("Course", request.Id));

        try
        {
            course.Publish();
        }
        catch (DomainException ex)
        {
            return Result.Failure(Error.Validation("Course", ex.Message));
        }

        courseRepository.Update(course);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

