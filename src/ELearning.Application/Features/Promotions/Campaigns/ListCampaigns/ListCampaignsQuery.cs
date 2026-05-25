using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;

public sealed record ListCampaignsQuery(
    Guid? OrganizationId,
    bool IncludeGlobal,
    int Page = 1,
    int PageSize = 20)
    : IRequest<Result<PagedList<CampaignListItemDto>>>;
