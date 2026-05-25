using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.GetReviewEligibility;

public sealed record GetReviewEligibilityQuery(Guid CourseId) : IRequest<Result<ReviewEligibilityDto>>;
