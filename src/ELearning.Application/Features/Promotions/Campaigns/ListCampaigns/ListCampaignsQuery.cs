using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;

public sealed record ListCampaignsQuery(
    Guid? OrganizationId,
    bool IncludeGlobal,
    int Take = 50)
    : IRequest<Result<IReadOnlyList<CampaignListItemDto>>>;

