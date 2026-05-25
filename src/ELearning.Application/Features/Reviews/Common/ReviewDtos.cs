using ELearning.Domain.Aggregates.ReviewAggregate;

namespace ELearning.Application.Features.Reviews.Common;

public sealed record ReviewDto(
    Guid Id,
    Guid CourseId,
    Guid UserId,
    int Rating,
    string Comment,
    string Status,
    DateTime SubmittedAt,
    DateTime? ModeratedAt,
    Guid? ModeratedByUserId,
    string? ModerationReason);

public sealed record CourseRatingSummaryDto(Guid CourseId, decimal AverageRating, int ReviewCount);

public static class ReviewMapper
{
    public static ReviewDto ToDto(Review review) => new(
        review.Id,
        review.CourseId,
        review.UserId,
        review.Rating,
        review.Comment,
        review.Status.ToString(),
        review.SubmittedAt,
        review.ModeratedAt,
        review.ModeratedByUserId,
        review.ModerationReason);
}
