using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.DeleteCourse;

public sealed class DeleteCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<DeleteCourseCommand, Result>
{
    public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdAsync(request.Id, ct);
        if (course is null)
            return Result.Failure(Error.NotFound("Course", request.Id));

        courseRepository.Remove(course);
        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveByPrefixAsync("courses:list", ct);
        await cache.RemoveAsync(cacheKeyBuilder.Build("courses", "detail", course.Id.ToString("N")), ct);

        return Result.Success();
    }
}
