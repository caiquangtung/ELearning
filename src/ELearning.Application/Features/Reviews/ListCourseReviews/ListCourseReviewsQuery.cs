using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.ListCourseReviews;

public sealed record ListCourseReviewsQuery(Guid CourseId, int Page = 1, int PageSize = 20, bool IncludeRejected = false)
    : IRequest<Result<PagedList<ReviewDto>>>;
