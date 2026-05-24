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

        if (!await certificateRepository.ExistsVerifiableForCourseAsync(userId.Value, request.CourseId, ct))
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
    }
}
