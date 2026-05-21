using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using MediatR;

namespace ELearning.Application.Features.Reviews.ListCourseReviews;

public sealed class ListCourseReviewsQueryHandler(
    ICourseRepository courseRepository,
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListCourseReviewsQuery, Result<PagedList<ReviewDto>>>
{
    public async Task<Result<PagedList<ReviewDto>>> Handle(ListCourseReviewsQuery request, CancellationToken ct)
    {
        if (!await courseRepository.ExistsAsync(c => c.Id == request.CourseId, ct))
            return Result.Failure<PagedList<ReviewDto>>(Error.NotFound("Course", request.CourseId));

        var paged = await reviewRepository.ListForCourseAsync(
            request.CourseId,
            request.Page,
            request.PageSize,
            request.IncludeRejected && currentUserService.HasRole(Roles.Admin),
            ct);

        return PagedList<ReviewDto>.Create(
            paged.Items.Select(ReviewMapper.ToDto).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }
}
