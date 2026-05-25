using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetCourseAnalytics;

public sealed class GetCourseAnalyticsQueryHandler(
    IReportingReadService reportingReadService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<GetCourseAnalyticsQuery, Result<CourseAnalyticsDto>>
{
    public async Task<Result<CourseAnalyticsDto>> Handle(GetCourseAnalyticsQuery request, CancellationToken ct)
    {
        var dto = await cache.GetOrCreateAsync<CourseAnalyticsDto?>(
            cacheKeyBuilder.Build("analytics", "course", request.CourseId.ToString("N")),
            token => reportingReadService.GetCourseAnalyticsAsync(request.CourseId, token),
            TimeSpan.FromMinutes(3),
            ct);
        return dto is null
            ? Result.Failure<CourseAnalyticsDto>(Error.NotFound("Course", request.CourseId))
            : dto;
    }
}
