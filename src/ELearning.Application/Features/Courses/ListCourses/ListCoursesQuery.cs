using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using MediatR;

namespace ELearning.Application.Features.Courses.ListCourses;

public sealed record ListCoursesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    CourseStatus? Status = null)
    : IRequest<Result<PagedList<CourseListItemDto>>>;

