using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.ListCourses;

public sealed class ListCoursesQueryHandler(
    ICourseRepository courseRepository,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<ListCoursesQuery, Result<PagedList<CourseListItemDto>>>
{
    public async Task<Result<PagedList<CourseListItemDto>>> Handle(ListCoursesQuery request, CancellationToken ct)
    {
        var key = cacheKeyBuilder.BuildHashKey("courses:list", request);

        var dto = await cache.GetOrCreateAsync(
            key,
            async token =>
            {
                var paged = await courseRepository.ListAsync(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.Status,
                    request.MinPriceCents,
                    request.MaxPriceCents,
                    request.Sort,
                    token);

                return PagedList<CourseListItemDto>.Create(
                    paged.Items.Select(c => new CourseListItemDto(
                        c.Id,
                        c.Title,
                        c.Status.ToString(),
                        c.PriceCents,
                        c.Currency,
                        c.CreatedAt)).ToList(),
                    paged.Page,
                    paged.PageSize,
                    paged.TotalCount);
            },
            TimeSpan.FromMinutes(5),
            ct);

        return dto;
    }
}
