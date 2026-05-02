using ELearning.Domain.Aggregates.PromotionAggregate;

namespace ELearning.Application.Features.Promotions.Common;

public static class CampaignDtoMapper
{
    public static CampaignDto ToDto(Campaign c) =>
        new(
            c.Id,
            c.Name,
            c.Scope.ToString(),
            c.OrganizationId,
            c.Status.ToString(),
            c.StartUtc,
            c.EndUtc,
            c.Rules.Select(r => new PromotionRuleDto(
                    r.Id,
                    r.RuleType.ToString(),
                    r.PercentOff,
                    r.AppliesToItemTypes.Select(t => t.ToString()).ToList()))
                .ToList(),
            c.Coupons.Select(cp => new CouponDto(
                    cp.Id,
                    cp.CampaignId,
                    cp.Code,
                    cp.Status.ToString(),
                    cp.ExpiresUtc,
                    cp.PerBuyerMaxRedemptions))
                .ToList());

    public static CampaignListItemDto ToListItem(Campaign c) =>
        new(
            c.Id,
            c.Name,
            c.Scope.ToString(),
            c.OrganizationId,
            c.Status.ToString(),
            c.StartUtc,
            c.EndUtc);
}

