using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.ListLicensePools;

public sealed class ListLicensePoolsQueryHandler(ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<ListLicensePoolsQuery, Result<PagedList<LicensePoolListItemDto>>>
{
    public async Task<Result<PagedList<LicensePoolListItemDto>>> Handle(ListLicensePoolsQuery request, CancellationToken ct)
    {
        var pools = await licensePoolRepository.ListByOrganizationAsync(
            request.OrganizationId,
            request.Page,
            request.PageSize,
            ct);

        var items = pools.Items
            .Select(p => new LicensePoolListItemDto(
                p.Id,
                p.OrganizationId,
                p.Name,
                p.TotalSeats,
                p.ActiveSeatCount,
                p.AvailableSeats,
                p.SeatPriceCents,
                p.Currency,
                p.ExpiresAt,
                p.CreatedAt))
            .ToList();

        return PagedList<LicensePoolListItemDto>.Create(items, pools.Page, pools.PageSize, pools.TotalCount);
    }
}
