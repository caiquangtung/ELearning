using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Licenses.RevokeLicense;

public sealed class RevokeLicenseCommandHandler(
    ILicensePoolRepository licensePoolRepository,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogs)
    : IRequestHandler<RevokeLicenseCommand, Result<LicenseUsageReportDto>>
{
    public async Task<Result<LicenseUsageReportDto>> Handle(RevokeLicenseCommand request, CancellationToken ct)
    {
        var pool = await licensePoolRepository.GetByIdWithAssignmentsAsync(request.LicensePoolId, ct);
        if (pool is null)
            return Result.Failure<LicenseUsageReportDto>(Error.NotFound("LicensePool", "License pool not found."));

        try
        {
            pool.RevokeSeat(request.UserId);
            licensePoolRepository.Update(pool);
            await unitOfWork.SaveChangesAsync(ct);
            await auditLogs.WriteAsync(new AuditLogEntry(
                "License.Revoke",
                "LicensePool",
                pool.Id.ToString(),
                "Success",
                new Dictionary<string, string> { ["revokedUserId"] = request.UserId.ToString() }), ct);

            return new LicenseUsageReportDto(pool.Id, pool.TotalSeats, pool.ActiveSeatCount, pool.AvailableSeats);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LicenseUsageReportDto>(Error.Conflict("LicensePool", ex.Message));
        }
    }
}
