using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Licenses.AssignLicense;

public sealed class AssignLicenseCommandHandler(
    ILicensePoolRepository licensePoolRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignLicenseCommand, Result<LicenseUsageReportDto>>
{
    public async Task<Result<LicenseUsageReportDto>> Handle(AssignLicenseCommand request, CancellationToken ct)
    {
        var pool = await licensePoolRepository.GetByIdWithAssignmentsAsync(request.LicensePoolId, ct);
        if (pool is null)
            return Result.Failure<LicenseUsageReportDto>(Error.NotFound("LicensePool", "License pool not found."));

        try
        {
            pool.AssignSeat(request.UserId);
            licensePoolRepository.Update(pool);
            await unitOfWork.SaveChangesAsync(ct);

            return new LicenseUsageReportDto(pool.Id, pool.TotalSeats, pool.ActiveSeatCount, pool.AvailableSeats);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LicenseUsageReportDto>(Error.Conflict("LicensePool", ex.Message));
        }
    }
}

