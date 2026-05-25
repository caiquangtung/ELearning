using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.PublishCourse;

public sealed class PublishCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
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
        await cache.RemoveByPrefixAsync("courses:list", ct);
        await cache.RemoveAsync(cacheKeyBuilder.Build("courses", "detail", course.Id.ToString("N")), ct);
        return Result.Success();
    }
}
