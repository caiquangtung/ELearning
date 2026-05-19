using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetAdminDashboard;

public sealed class GetAdminDashboardQueryHandler(IReportingReadService reportingReadService)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken ct) =>
        await reportingReadService.GetAdminDashboardAsync(ct);
}
