using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetOrganizationAnalytics;

public sealed class GetOrganizationAnalyticsQueryHandler(IReportingReadService reportingReadService)
    : IRequestHandler<GetOrganizationAnalyticsQuery, Result<OrganizationAnalyticsDto>>
{
    public async Task<Result<OrganizationAnalyticsDto>> Handle(GetOrganizationAnalyticsQuery request, CancellationToken ct)
    {
        var dto = await reportingReadService.GetOrganizationAnalyticsAsync(request.OrganizationId, ct);
        return dto is null
            ? Result.Failure<OrganizationAnalyticsDto>(Error.NotFound("Organization", request.OrganizationId))
            : dto;
    }
}
