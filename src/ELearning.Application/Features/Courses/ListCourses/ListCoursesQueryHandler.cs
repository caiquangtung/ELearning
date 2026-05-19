using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.ListCourses;

public sealed class ListCoursesQueryHandler(ICourseRepository courseRepository)
    : IRequestHandler<ListCoursesQuery, Result<PagedList<CourseListItemDto>>>
{
    public async Task<Result<PagedList<CourseListItemDto>>> Handle(ListCoursesQuery request, CancellationToken ct)
    {
        var paged = await courseRepository.ListAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Status,
            request.MinPriceCents,
            request.MaxPriceCents,
            request.Sort,
            ct);

        var dto = PagedList<CourseListItemDto>.Create(
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

        return dto;
    }
}
