using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.PromotionAggregate;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.Analytics;

public sealed class GetCampaignAnalyticsQueryHandler(
    ICampaignRepository campaigns,
    ICouponRedemptionRepository redemptions,
    IOrderRepository orders)
    : IRequestHandler<GetCampaignAnalyticsQuery, Result<CampaignAnalyticsDto>>
{
    public async Task<Result<CampaignAnalyticsDto>> Handle(GetCampaignAnalyticsQuery request, CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdWithRulesAndCouponsAsync(request.CampaignId, ct);
        if (campaign is null)
            return Result.Failure<CampaignAnalyticsDto>(Error.NotFound(nameof(Campaign), request.CampaignId));

        var couponIds = campaign.Coupons.Select(c => c.Id).ToList();
        if (couponIds.Count == 0)
            return Result.Success(new CampaignAnalyticsDto(request.CampaignId, 0, 0, 0, null));

        // Pull redemptions for campaign coupons and compute aggregates in-memory (MVP).
        var redemptionRows = await redemptions.FindAsync(r => couponIds.Contains(r.CouponId), ct);
        if (redemptionRows.Count == 0)
            return Result.Success(new CampaignAnalyticsDto(request.CampaignId, 0, 0, 0, null));

        var uniqueBuyers = redemptionRows.Select(r => r.BuyerUserId).Distinct().Count();
        var last = redemptionRows.MaxBy(r => r.RedeemedAtUtc)?.RedeemedAtUtc;

        // Sum discounts from orders that used coupons in this campaign.
        // (MVP) Uses `Order.DiscountCents` as the “promo impact”.
        var orderIds = redemptionRows.Where(r => r.OrderId is not null).Select(r => r.OrderId!.Value).Distinct().ToList();
        long totalDiscount = 0;
        if (orderIds.Count > 0)
        {
            var usedOrders = await orders.FindAsync(o => orderIds.Contains(o.Id), ct);
            totalDiscount = usedOrders.Sum(o => o.DiscountCents);
        }

        return Result.Success(new CampaignAnalyticsDto(
            request.CampaignId,
            redemptionRows.Count,
            uniqueBuyers,
            totalDiscount,
            last));
    }
}

