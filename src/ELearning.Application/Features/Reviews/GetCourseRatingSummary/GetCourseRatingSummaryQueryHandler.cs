using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.GetCourseRatingSummary;

public sealed class GetCourseRatingSummaryQueryHandler(ICourseRepository courseRepository, IReviewRepository reviewRepository)
    : IRequestHandler<GetCourseRatingSummaryQuery, Result<CourseRatingSummaryDto>>
{
    public async Task<Result<CourseRatingSummaryDto>> Handle(GetCourseRatingSummaryQuery request, CancellationToken ct)
    {
        if (!await courseRepository.ExistsAsync(c => c.Id == request.CourseId, ct))
            return Result.Failure<CourseRatingSummaryDto>(Error.NotFound("Course", request.CourseId));

        var summary = await reviewRepository.GetSummaryForCourseAsync(request.CourseId, ct);
        return new CourseRatingSummaryDto(request.CourseId, summary.AverageRating, summary.ReviewCount);
    }
}
