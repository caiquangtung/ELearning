using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.Preview;

public sealed record PreviewCampaignQuoteItem(
    string ItemType,
    Guid ReferenceId,
    int Quantity);

public sealed record PreviewCampaignQuoteQuery(
    Guid CampaignId,
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<PreviewCampaignQuoteItem> Items,
    string? CouponCode)
    : IRequest<Result<PromotionQuoteDto>>;

