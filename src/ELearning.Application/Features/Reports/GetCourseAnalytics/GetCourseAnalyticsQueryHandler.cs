using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetCourseAnalytics;

public sealed class GetCourseAnalyticsQueryHandler(IReportingReadService reportingReadService)
    : IRequestHandler<GetCourseAnalyticsQuery, Result<CourseAnalyticsDto>>
{
    public async Task<Result<CourseAnalyticsDto>> Handle(GetCourseAnalyticsQuery request, CancellationToken ct)
    {
        var dto = await reportingReadService.GetCourseAnalyticsAsync(request.CourseId, ct);
        return dto is null
            ? Result.Failure<CourseAnalyticsDto>(Error.NotFound("Course", request.CourseId))
            : dto;
    }
}
