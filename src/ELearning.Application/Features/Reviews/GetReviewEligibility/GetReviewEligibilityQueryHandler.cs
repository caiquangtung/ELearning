using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.GetReviewEligibility;

public sealed class GetReviewEligibilityQueryHandler(
    ICourseRepository courseRepository,
    ICertificateRepository certificateRepository,
    IVideoAssetRepository videoAssetRepository,
    IWatchEventRepository watchEventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetReviewEligibilityQuery, Result<ReviewEligibilityDto>>
{
    public async Task<Result<ReviewEligibilityDto>> Handle(GetReviewEligibilityQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<ReviewEligibilityDto>(Error.Unauthorized());

        if (!await courseRepository.ExistsAsync(c => c.Id == request.CourseId, ct))
            return Result.Failure<ReviewEligibilityDto>(Error.NotFound("Course", request.CourseId));

        var canReview = await ReviewEligibility.CanReviewAsync(
            request.CourseId,
            userId.Value,
            courseRepository,
            certificateRepository,
            videoAssetRepository,
            watchEventRepository,
            ct);

        return new ReviewEligibilityDto(
            request.CourseId,
            canReview,
            canReview ? null : ReviewEligibility.CompletionRequiredReason);
    }
}
