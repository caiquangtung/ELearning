using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Licenses.CreateLicensePool;

public sealed class CreateLicensePoolCommandHandler(
    ILicensePoolRepository licensePoolRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLicensePoolCommand, Result<LicensePoolDetailDto>>
{
    public async Task<Result<LicensePoolDetailDto>> Handle(CreateLicensePoolCommand request, CancellationToken ct)
    {
        try
        {
            var pool = LicensePool.Create(request.OrganizationId, request.Name, request.TotalSeats, request.ExpiresAt);
            licensePoolRepository.Add(pool);
            await unitOfWork.SaveChangesAsync(ct);

            return new LicensePoolDetailDto(
                pool.Id,
                pool.OrganizationId,
                pool.Name,
                pool.TotalSeats,
                pool.ActiveSeatCount,
                pool.AvailableSeats,
                pool.ExpiresAt,
                pool.CreatedAt,
                []);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LicensePoolDetailDto>(Error.Conflict("LicensePool", ex.Message));
        }
    }
}

