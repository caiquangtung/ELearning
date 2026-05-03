using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Exceptions;

namespace ELearning.Application.Features.Promotions.Common;

public sealed class PromotionDiscountCalculator(
    ICouponRepository couponRepository,
    ICampaignRepository campaignRepository,
    ICouponRedemptionRepository redemptionRepository)
{
    public async Task<(long DiscountCents, string? AppliedCouponCode)> CalculateDiscountAsync(
        Guid buyerUserId,
        Guid? organizationId,
        string currency,
        IReadOnlyList<(OrderItemType ItemType, long LineTotalCents)> pricedLines,
        string? couponCode,
        bool isB2B,
        IReadOnlyList<(OrderItemType ItemType, int Quantity, long UnitPriceCents)> quantityLines,
        CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;

        // Eligible campaigns (auto-apply): global + org-specific
        var eligibleCampaigns = await campaignRepository.FindAsync(
            c =>
                c.Status == CampaignStatus.Active
                && c.StartUtc <= utcNow
                && (c.EndUtc == null || c.EndUtc > utcNow)
                && ((organizationId != null && c.OrganizationId == organizationId) || c.OrganizationId == null),
            ct);

        Coupon? coupon = null;
        Campaign? couponCampaign = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var normalized = Coupon.NormalizeCode(couponCode);
            coupon = await couponRepository.GetByCodeNormalizedAsync(normalized, ct);
            if (coupon is null) throw new DomainException("Invalid coupon code.");
            if (!coupon.IsValidAt(utcNow)) throw new DomainException("Coupon is expired or disabled.");

            var redemptionCount = await redemptionRepository.CountForBuyerAsync(coupon.Id, buyerUserId, ct);
            if (redemptionCount >= coupon.PerBuyerMaxRedemptions)
                throw new DomainException("Coupon redemption limit reached for this buyer.");

            couponCampaign = await campaignRepository.GetByIdWithRulesAndCouponsAsync(coupon.CampaignId, ct)
                ?? throw new DomainException("Campaign not found for coupon.");

            if (!couponCampaign.IsEligibleFor(organizationId, utcNow))
                throw new DomainException("Campaign is not eligible for this order.");
        }

        // Stacking rule MVP: take the MAX percent discount per item across all eligible campaigns (incl. coupon campaign).
        var allCampaigns = couponCampaign is null
            ? eligibleCampaigns
            : eligibleCampaigns.Concat([couponCampaign]).DistinctBy(c => c.Id).ToList();

        long campaignDiscount = 0;
        foreach (var line in pricedLines)
        {
            var bestPercent = 0;
            foreach (var c in allCampaigns)
            {
                foreach (var r in c.Rules.Where(r => r.RuleType == PromotionRuleType.ItemPercentOff))
                {
                    if (!r.AppliesToItemTypes.Contains(line.ItemType)) continue;
                    bestPercent = Math.Max(bestPercent, r.PercentOff);
                }
            }

            if (bestPercent <= 0) continue;
            campaignDiscount += (long)Math.Floor(line.LineTotalCents * (bestPercent / 100.0));
        }

        // Volume discount MVP (B2B): tier pricing on LicensePool quantities.
        // Tiers: >=50 seats => 10% off, >=20 seats => 5% off.
        long volumeDiscount = 0;
        if (isB2B)
        {
            foreach (var q in quantityLines.Where(x => x.ItemType == OrderItemType.LicensePool))
            {
                var tierPercent = q.Quantity >= 50 ? 10 : q.Quantity >= 20 ? 5 : 0;
                if (tierPercent <= 0) continue;
                var lineTotal = q.UnitPriceCents * q.Quantity;
                volumeDiscount += (long)Math.Floor(lineTotal * (tierPercent / 100.0));
            }
        }

        var subtotal = pricedLines.Sum(l => l.LineTotalCents);
        var discount = Math.Clamp(campaignDiscount + volumeDiscount, 0, subtotal);

        return (discount, coupon?.Code);
    }
}

