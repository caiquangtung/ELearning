using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.GetLicensePool;

public sealed class GetLicensePoolQueryHandler(ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<GetLicensePoolQuery, Result<LicensePoolDetailDto>>
{
    public async Task<Result<LicensePoolDetailDto>> Handle(GetLicensePoolQuery request, CancellationToken ct)
    {
        var pool = await licensePoolRepository.GetByIdWithAssignmentsAsync(request.LicensePoolId, ct);
        if (pool is null)
            return Result.Failure<LicensePoolDetailDto>(Error.NotFound("LicensePool", "License pool not found."));

        var assignments = pool.Assignments
            .OrderByDescending(a => a.AssignedAt)
            .Select(a => new LicenseAssignmentDto(a.UserId, a.AssignedAt, a.RevokedAt))
            .ToList();

        return new LicensePoolDetailDto(
            pool.Id,
            pool.OrganizationId,
            pool.Name,
            pool.TotalSeats,
            pool.ActiveSeatCount,
            pool.AvailableSeats,
            pool.ExpiresAt,
            pool.CreatedAt,
            assignments);
    }
}

