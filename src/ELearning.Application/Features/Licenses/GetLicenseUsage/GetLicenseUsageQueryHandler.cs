using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.GetLicenseUsage;

public sealed class GetLicenseUsageQueryHandler(ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<GetLicenseUsageQuery, Result<LicenseUsageReportDto>>
{
    public async Task<Result<LicenseUsageReportDto>> Handle(GetLicenseUsageQuery request, CancellationToken ct)
    {
        var pool = await licensePoolRepository.GetByIdWithAssignmentsAsync(request.LicensePoolId, ct);
        if (pool is null)
            return Result.Failure<LicenseUsageReportDto>(Error.NotFound("LicensePool", "License pool not found."));

        return new LicenseUsageReportDto(pool.Id, pool.TotalSeats, pool.ActiveSeatCount, pool.AvailableSeats);
    }
}

