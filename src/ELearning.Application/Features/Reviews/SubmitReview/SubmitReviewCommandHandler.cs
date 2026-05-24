using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Reviews.SubmitReview;

public sealed class SubmitReviewCommandHandler(
    ICourseRepository courseRepository,
    ICertificateRepository certificateRepository,
    IVideoAssetRepository videoAssetRepository,
    IWatchEventRepository watchEventRepository,
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitReviewCommand, Result<ReviewDto>>
{
    public async Task<Result<ReviewDto>> Handle(SubmitReviewCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<ReviewDto>(Error.Unauthorized());

        if (!await courseRepository.ExistsAsync(c => c.Id == request.CourseId, ct))
            return Result.Failure<ReviewDto>(Error.NotFound("Course", request.CourseId));

        var canReview = await ReviewEligibility.CanReviewAsync(
            request.CourseId,
            userId.Value,
            courseRepository,
            certificateRepository,
            videoAssetRepository,
            watchEventRepository,
            ct);

        if (!canReview)
            return Result.Failure<ReviewDto>(
                Error.Forbidden("Course completion is required before submitting a review."));

        try
        {
            var review = await reviewRepository.GetForCourseAndUserAsync(request.CourseId, userId.Value, ct);
            if (review is null)
            {
                review = Review.Submit(request.CourseId, userId.Value, request.Rating, request.Comment);
                reviewRepository.Add(review);
            }
            else
            {
                review.Update(request.Rating, request.Comment);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ReviewMapper.ToDto(review);
        }
        catch (DomainException ex)
        {
            return Result.Failure<ReviewDto>(Error.Validation("Review", ex.Message));
        }
        catch (Exception ex) when (IsUniqueReviewConflict(ex))
        {
            return Result.Failure<ReviewDto>(
                Error.Conflict("Review", "A review already exists for this learner and course. Please reload and edit the existing review."));
        }
    }

    private static bool IsUniqueReviewConflict(Exception ex)
        => ex.GetType().Name == "DbUpdateException"
           && (ex.InnerException?.Message.Contains("IX_reviews_course_id_user_id", StringComparison.OrdinalIgnoreCase) == true
               || ex.Message.Contains("IX_reviews_course_id_user_id", StringComparison.OrdinalIgnoreCase));
}
