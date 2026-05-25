using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.SubmitReview;

public sealed record SubmitReviewCommand(Guid CourseId, int Rating, string Comment)
    : IRequest<Result<ReviewDto>>;
