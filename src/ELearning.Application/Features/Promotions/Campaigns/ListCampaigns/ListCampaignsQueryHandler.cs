using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;

public sealed class ListCampaignsQueryHandler(ICampaignRepository campaigns)
    : IRequestHandler<ListCampaignsQuery, Result<PagedList<CampaignListItemDto>>>
{
    public async Task<Result<PagedList<CampaignListItemDto>>> Handle(ListCampaignsQuery request, CancellationToken ct)
    {
        var rows = await campaigns.ListAsync(
            request.OrganizationId,
            request.IncludeGlobal,
            request.Page,
            request.PageSize,
            ct);

        var items = rows.Items
            .Select(CampaignDtoMapper.ToListItem)
            .ToList();

        return PagedList<CampaignListItemDto>.Create(items, rows.Page, rows.PageSize, rows.TotalCount);
    }
}
