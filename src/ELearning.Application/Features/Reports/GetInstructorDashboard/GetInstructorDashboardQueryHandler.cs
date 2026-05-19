using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetInstructorDashboard;

public sealed class GetInstructorDashboardQueryHandler(
    IReportingReadService reportingReadService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetInstructorDashboardQuery, Result<InstructorDashboardDto>>
{
    public async Task<Result<InstructorDashboardDto>> Handle(GetInstructorDashboardQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<InstructorDashboardDto>(Error.Unauthorized());

        return await reportingReadService.GetInstructorDashboardAsync(userId.Value, ct);
    }
}
