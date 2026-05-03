using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.Preview;

public sealed class PreviewCampaignQuoteQueryHandler(
    ICampaignRepository campaigns,
    ICouponRepository coupons,
    ICouponRedemptionRepository redemptions,
    ICourseRepository courseRepository,
    ITrainingClassRepository trainingClassRepository,
    ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<PreviewCampaignQuoteQuery, Result<PromotionQuoteDto>>
{
    public async Task<Result<PromotionQuoteDto>> Handle(PreviewCampaignQuoteQuery request, CancellationToken ct)
    {
        try
        {
            if (request.BuyerUserId == Guid.Empty)
                return Result.Failure<PromotionQuoteDto>(Error.Validation("BuyerUserId", "BuyerUserId is required."));
            if (string.IsNullOrWhiteSpace(request.Currency))
                return Result.Failure<PromotionQuoteDto>(Error.Validation("Currency", "Currency is required."));
            if (request.Items.Count == 0)
                return Result.Failure<PromotionQuoteDto>(Error.Validation("Items", "At least one item is required."));

            var currency = request.Currency.Trim().ToUpperInvariant();
            var campaign = await campaigns.GetByIdWithRulesAndCouponsAsync(request.CampaignId, ct);
            if (campaign is null)
                return Result.Failure<PromotionQuoteDto>(Error.NotFound(nameof(Campaign), request.CampaignId));

            var items = new List<PromotionQuoteItemDto>(request.Items.Count);
            long subtotal = 0;
            var quantityLines = new List<(OrderItemType ItemType, int Quantity, long UnitPriceCents)>();
            var pricedLines = new List<(OrderItemType ItemType, long LineTotalCents)>();

            foreach (var raw in request.Items)
            {
                if (!Enum.TryParse<OrderItemType>(raw.ItemType.Trim(), ignoreCase: true, out var itemType))
                    return Result.Failure<PromotionQuoteDto>(Error.Validation("ItemType", "Invalid ItemType."));

                var priced = await GetUnitPriceAsync(itemType, raw.ReferenceId, ct);
                if (!priced.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                    return Result.Failure<PromotionQuoteDto>(
                        Error.Conflict(
                            "Order.CurrencyMismatch",
                            $"Item currency {priced.Currency} does not match order currency {currency}."));

                var lineTotal = priced.UnitPriceCents * raw.Quantity;
                subtotal += lineTotal;
                quantityLines.Add((itemType, raw.Quantity, priced.UnitPriceCents));
                pricedLines.Add((itemType, lineTotal));
                items.Add(new PromotionQuoteItemDto(raw.ItemType.Trim(), raw.ReferenceId, raw.Quantity, priced.UnitPriceCents, lineTotal, 0));
            }

            // Preview stacking: include ONLY the selected campaign + optional coupon campaign.
            Coupon? coupon = null;
            Campaign? couponCampaign = null;
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var normalized = Coupon.NormalizeCode(request.CouponCode);
                coupon = await coupons.GetByCodeNormalizedAsync(normalized, ct);
                if (coupon is null) throw new DomainException("Invalid coupon code.");
                if (!coupon.IsValidAt(DateTime.UtcNow)) throw new DomainException("Coupon is expired or disabled.");

                var count = await redemptions.CountForBuyerAsync(coupon.Id, request.BuyerUserId, ct);
                if (count >= coupon.PerBuyerMaxRedemptions)
                    throw new DomainException("Coupon redemption limit reached for this buyer.");

                couponCampaign = await campaigns.GetByIdWithRulesAndCouponsAsync(coupon.CampaignId, ct)
                    ?? throw new DomainException("Campaign not found for coupon.");
            }

            var all = couponCampaign is null
                ? new List<Campaign> { campaign }
                : new List<Campaign> { campaign, couponCampaign }.DistinctBy(c => c.Id).ToList();

            long campaignDiscount = 0;
            foreach (var line in pricedLines)
            {
                var bestPercent = 0;
                foreach (var c in all)
                {
                    foreach (var r in c.Rules.Where(r => r.RuleType == PromotionRuleType.ItemPercentOff))
                    {
                        if (!r.AppliesToItemTypes.Contains(line.ItemType)) continue;
                        bestPercent = Math.Max(bestPercent, r.PercentOff);
                    }
                }
                if (bestPercent > 0)
                    campaignDiscount += (long)Math.Floor(line.LineTotalCents * (bestPercent / 100.0));
            }

            long volumeDiscount = 0;
            if (request.OrganizationId is not null)
            {
                foreach (var q in quantityLines.Where(x => x.ItemType == OrderItemType.LicensePool))
                {
                    var tierPercent = q.Quantity >= 50 ? 10 : q.Quantity >= 20 ? 5 : 0;
                    if (tierPercent <= 0) continue;
                    volumeDiscount += (long)Math.Floor((q.UnitPriceCents * q.Quantity) * (tierPercent / 100.0));
                }
            }

            var discount = Math.Clamp(campaignDiscount + volumeDiscount, 0, subtotal);
            var total = Math.Max(0, subtotal - discount);

            if (discount > 0 && subtotal > 0)
            {
                long allocated = 0;
                for (var idx = 0; idx < items.Count; idx++)
                {
                    var it = items[idx];
                    var d = idx == items.Count - 1
                        ? discount - allocated
                        : (long)Math.Floor(discount * (it.LineTotalCents / (double)subtotal));
                    allocated += d;
                    items[idx] = it with { DiscountCents = d };
                }
            }

            return Result.Success(new PromotionQuoteDto(
                currency,
                subtotal,
                discount,
                total,
                coupon?.Code,
                items));
        }
        catch (DomainException ex)
        {
            return Result.Failure<PromotionQuoteDto>(Error.Conflict("CampaignPreview", ex.Message));
        }
    }

    private async Task<(long UnitPriceCents, string Currency)> GetUnitPriceAsync(
        OrderItemType itemType,
        Guid referenceId,
        CancellationToken ct)
    {
        return itemType switch
        {
            OrderItemType.Course => await GetCoursePriceAsync(referenceId, ct),
            OrderItemType.TrainingClass => await GetTrainingClassPriceAsync(referenceId, ct),
            OrderItemType.LicensePool => await GetLicensePoolSeatPriceAsync(referenceId, ct),
            _ => throw new DomainException("Unsupported item type.")
        };
    }

    private async Task<(long UnitPriceCents, string Currency)> GetCoursePriceAsync(Guid courseId, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdAsync(courseId, ct);
        if (course is null) throw new DomainException("Course not found.");
        if (course.PriceCents <= 0) throw new DomainException("Course price is not set.");
        return (course.PriceCents, course.Currency);
    }

    private async Task<(long UnitPriceCents, string Currency)> GetTrainingClassPriceAsync(Guid classId, CancellationToken ct)
    {
        var tc = await trainingClassRepository.GetByIdAsync(classId, ct);
        if (tc is null) throw new DomainException("Training class not found.");
        if (tc.PriceCents <= 0) throw new DomainException("Training class price is not set.");
        return (tc.PriceCents, tc.Currency);
    }

    private async Task<(long UnitPriceCents, string Currency)> GetLicensePoolSeatPriceAsync(Guid poolId, CancellationToken ct)
    {
        var pool = await licensePoolRepository.GetByIdWithAssignmentsAsync(poolId, ct);
        if (pool is null) throw new DomainException("License pool not found.");
        if (pool.SeatPriceCents <= 0) throw new DomainException("License pool seat price is not set.");
        return (pool.SeatPriceCents, pool.Currency);
    }
}

