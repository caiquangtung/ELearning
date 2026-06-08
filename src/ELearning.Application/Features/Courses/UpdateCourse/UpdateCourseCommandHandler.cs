using ELearning.Application.Features.Courses.Common;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.UpdateCourse;

public sealed class UpdateCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder,
    IAiKnowledgeReindexQueue knowledgeReindexQueue)
    : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    public async Task<Result<CourseDto>> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdAsync(request.Id, ct);
        if (course is null)
            return Result.Failure<CourseDto>(Error.NotFound("Course", request.Id));

        try
        {
            course.Update(request.Title, request.Description);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CourseDto>(Error.Validation("Course", ex.Message));
        }

        courseRepository.Update(course);
        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveByPrefixAsync("courses:list", ct);
        await cache.RemoveAsync(cacheKeyBuilder.Build("courses", "detail", course.Id.ToString("N")), ct);
        if (course.Status == CourseStatus.Published)
            await knowledgeReindexQueue.EnqueueAsync(course.Id, ct);

        return new CourseDto(
            course.Id,
            course.Title,
            course.Description,
            course.Status.ToString(),
            course.CreatedAt,
            course.UpdatedAt);
    }
}
