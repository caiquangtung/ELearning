using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.QuoteCheckout;

public sealed class QuoteCheckoutQueryHandler(
    ICouponRepository couponRepository,
    ICampaignRepository campaignRepository,
    ICouponRedemptionRepository redemptionRepository,
    ICourseRepository courseRepository,
    ITrainingClassRepository trainingClassRepository,
    ILicensePoolRepository licensePoolRepository)
    : IRequestHandler<QuoteCheckoutQuery, Result<PromotionQuoteDto>>
{
    public async Task<Result<PromotionQuoteDto>> Handle(QuoteCheckoutQuery request, CancellationToken ct)
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
            var utcNow = DateTime.UtcNow;

            Coupon? coupon = null;
            Campaign? campaign = null;
            IReadOnlyList<PromotionRule> rules = Array.Empty<PromotionRule>();

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var codeNormalized = Coupon.NormalizeCode(request.CouponCode);
                coupon = await couponRepository.GetByCodeNormalizedAsync(codeNormalized, ct);
                if (coupon is null)
                    return Result.Failure<PromotionQuoteDto>(Error.Validation("CouponCode", "Invalid coupon code."));

                if (!coupon.IsValidAt(utcNow))
                    return Result.Failure<PromotionQuoteDto>(Error.Conflict("Coupon", "Coupon is expired or disabled."));

                campaign = await campaignRepository.GetByIdWithRulesAndCouponsAsync(coupon.CampaignId, ct);
                if (campaign is null)
                    return Result.Failure<PromotionQuoteDto>(Error.NotFound("Campaign", coupon.CampaignId));

                if (!campaign.IsEligibleFor(request.OrganizationId, utcNow))
                    return Result.Failure<PromotionQuoteDto>(Error.Conflict("Campaign", "Campaign is not eligible for this order."));

                rules = campaign.Rules;

                var redemptionCount = await redemptionRepository.CountForBuyerAsync(coupon.Id, request.BuyerUserId, ct);
                if (redemptionCount >= coupon.PerBuyerMaxRedemptions)
                    return Result.Failure<PromotionQuoteDto>(Error.Conflict("Coupon", "Coupon redemption limit reached for this buyer."));
            }

            var items = new List<PromotionQuoteItemDto>(request.Items.Count);
            long subtotal = 0;
            long discount = 0;

            foreach (var raw in request.Items)
            {
                if (string.IsNullOrWhiteSpace(raw.ItemType))
                    return Result.Failure<PromotionQuoteDto>(Error.Validation("ItemType", "ItemType is required."));
                if (raw.ReferenceId == Guid.Empty)
                    return Result.Failure<PromotionQuoteDto>(Error.Validation("ReferenceId", "ReferenceId is required."));
                if (raw.Quantity <= 0)
                    return Result.Failure<PromotionQuoteDto>(Error.Validation("Quantity", "Quantity must be greater than 0."));

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

                var itemDiscount = 0L;
                if (coupon is not null && rules.Count > 0)
                {
                    foreach (var rule in rules.Where(r => r.RuleType == PromotionRuleType.ItemPercentOff))
                    {
                        if (!rule.AppliesToItemTypes.Contains(itemType))
                            continue;

                        var d = (long)Math.Floor(lineTotal * (rule.PercentOff / 100.0));
                        itemDiscount = Math.Max(itemDiscount, d);
                    }
                }

                discount += itemDiscount;
                items.Add(new PromotionQuoteItemDto(raw.ItemType.Trim(), raw.ReferenceId, raw.Quantity, priced.UnitPriceCents, lineTotal, itemDiscount));
            }

            discount = Math.Clamp(discount, 0, subtotal);
            var total = Math.Max(0, subtotal - discount);

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
            return Result.Failure<PromotionQuoteDto>(Error.Conflict("Promotion", ex.Message));
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

