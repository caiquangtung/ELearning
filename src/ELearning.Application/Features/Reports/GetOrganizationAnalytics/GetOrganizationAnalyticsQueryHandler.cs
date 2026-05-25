using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetOrganizationAnalytics;

public sealed class GetOrganizationAnalyticsQueryHandler(
    IReportingReadService reportingReadService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<GetOrganizationAnalyticsQuery, Result<OrganizationAnalyticsDto>>
{
    public async Task<Result<OrganizationAnalyticsDto>> Handle(GetOrganizationAnalyticsQuery request, CancellationToken ct)
    {
        var dto = await cache.GetOrCreateAsync<OrganizationAnalyticsDto?>(
            cacheKeyBuilder.Build("analytics", "organization", request.OrganizationId.ToString("N")),
            token => reportingReadService.GetOrganizationAnalyticsAsync(request.OrganizationId, token),
            TimeSpan.FromMinutes(3),
            ct);
        return dto is null
            ? Result.Failure<OrganizationAnalyticsDto>(Error.NotFound("Organization", request.OrganizationId))
            : dto;
    }
}
