using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetStudentDashboard;

public sealed class GetStudentDashboardQueryHandler(
    IReportingReadService reportingReadService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardDto>>
{
    public async Task<Result<StudentDashboardDto>> Handle(GetStudentDashboardQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<StudentDashboardDto>(Error.Unauthorized());

        return await reportingReadService.GetStudentDashboardAsync(userId.Value, ct);
    }
}
