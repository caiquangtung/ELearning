using ELearning.Application.Features.Reviews.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reviews.GetCourseRatingSummary;

public sealed record GetCourseRatingSummaryQuery(Guid CourseId) : IRequest<Result<CourseRatingSummaryDto>>;
