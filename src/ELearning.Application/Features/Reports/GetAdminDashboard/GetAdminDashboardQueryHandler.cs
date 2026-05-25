using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetAdminDashboard;

public sealed class GetAdminDashboardQueryHandler(
    IReportingReadService reportingReadService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken ct) =>
        await cache.GetOrCreateAsync(
            cacheKeyBuilder.Build("analytics", "dashboard", "admin"),
            token => reportingReadService.GetAdminDashboardAsync(token),
            TimeSpan.FromMinutes(3),
            ct);
}
