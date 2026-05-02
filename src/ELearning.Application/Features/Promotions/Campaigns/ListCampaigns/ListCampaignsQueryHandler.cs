using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;

public sealed class ListCampaignsQueryHandler(ICampaignRepository campaigns)
    : IRequestHandler<ListCampaignsQuery, Result<IReadOnlyList<CampaignListItemDto>>>
{
    public async Task<Result<IReadOnlyList<CampaignListItemDto>>> Handle(ListCampaignsQuery request, CancellationToken ct)
    {
        var take = request.Take is <= 0 or > 200 ? 50 : request.Take;
        var rows = await campaigns.FindAsync(
            c =>
                (request.OrganizationId != null && c.OrganizationId == request.OrganizationId)
                || (request.IncludeGlobal && c.OrganizationId == null),
            ct);

        return rows
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .Select(CampaignDtoMapper.ToListItem)
            .ToList();
    }
}

