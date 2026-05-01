using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.ListLicensePools;

public sealed class ListLicensePoolsQueryHandler(ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<ListLicensePoolsQuery, Result<IReadOnlyList<LicensePoolListItemDto>>>
{
    public async Task<Result<IReadOnlyList<LicensePoolListItemDto>>> Handle(ListLicensePoolsQuery request, CancellationToken ct)
    {
        var pools = await licensePoolRepository.ListByOrganizationAsync(request.OrganizationId, ct);

        var items = pools
            .Select(p => new LicensePoolListItemDto(
                p.Id,
                p.OrganizationId,
                p.Name,
                p.TotalSeats,
                p.ActiveSeatCount,
                p.AvailableSeats,
                p.ExpiresAt,
                p.CreatedAt))
            .ToList();

        return items;
    }
}

