using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.ReviewAggregate;
using MediatR;

namespace ELearning.Application.Features.Reviews.ModerateReview;

public sealed record ModerateReviewCommand(Guid ReviewId, ReviewStatus Status, string? Reason)
    : IRequest<Result<ReviewDto>>;
