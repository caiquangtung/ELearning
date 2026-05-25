using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetInstructorDashboard;

public sealed class GetInstructorDashboardQueryHandler(
    IReportingReadService reportingReadService,
    ICurrentUserService currentUserService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder)
    : IRequestHandler<GetInstructorDashboardQuery, Result<InstructorDashboardDto>>
{
    public async Task<Result<InstructorDashboardDto>> Handle(GetInstructorDashboardQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<InstructorDashboardDto>(Error.Unauthorized());

        return await cache.GetOrCreateAsync(
            cacheKeyBuilder.Build("analytics", "dashboard", "instructor", userId.Value.ToString("N")),
            token => reportingReadService.GetInstructorDashboardAsync(userId.Value, token),
            TimeSpan.FromMinutes(3),
            ct);
    }
}
