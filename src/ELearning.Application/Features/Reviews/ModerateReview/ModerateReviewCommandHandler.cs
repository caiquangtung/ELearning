using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Reviews.ModerateReview;

public sealed class ModerateReviewCommandHandler(
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IAuditLogService auditLogs)
    : IRequestHandler<ModerateReviewCommand, Result<ReviewDto>>
{
    public async Task<Result<ReviewDto>> Handle(ModerateReviewCommand request, CancellationToken ct)
    {
        var moderatorUserId = currentUserService.UserId;
        if (!moderatorUserId.HasValue)
            return Result.Failure<ReviewDto>(Error.Unauthorized());

        var review = await reviewRepository.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result.Failure<ReviewDto>(Error.NotFound("Review", request.ReviewId));

        try
        {
            if (request.Status == ReviewStatus.Published)
                review.Approve(moderatorUserId.Value);
            else
                review.Reject(moderatorUserId.Value, request.Reason ?? string.Empty);

            await unitOfWork.SaveChangesAsync(ct);
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Review.Moderate",
                "Review",
                review.Id.ToString(),
                "Success",
                new Dictionary<string, string>
                {
                    ["status"] = request.Status.ToString(),
                    ["courseId"] = review.CourseId.ToString()
                }), ct);
            await cache.RemoveByPrefixAsync("courses:list", ct);
            return ReviewMapper.ToDto(review);
        }
        catch (DomainException ex)
        {
            return Result.Failure<ReviewDto>(Error.Validation("Review", ex.Message));
        }
    }
}
