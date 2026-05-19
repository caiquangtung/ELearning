using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetCourseAnalytics;

public sealed record GetCourseAnalyticsQuery(Guid CourseId) : IRequest<Result<CourseAnalyticsDto>>;
