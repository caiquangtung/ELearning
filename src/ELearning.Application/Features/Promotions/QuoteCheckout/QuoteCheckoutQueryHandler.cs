using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
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
            var items = new List<PromotionQuoteItemDto>(request.Items.Count);
            long subtotal = 0;
            var quantityLines = new List<(OrderItemType ItemType, int Quantity, long UnitPriceCents)>();
            var pricedLines = new List<(OrderItemType ItemType, long LineTotalCents)>();

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
                quantityLines.Add((itemType, raw.Quantity, priced.UnitPriceCents));
                pricedLines.Add((itemType, lineTotal));
                items.Add(new PromotionQuoteItemDto(raw.ItemType.Trim(), raw.ReferenceId, raw.Quantity, priced.UnitPriceCents, lineTotal, 0));
            }

            var calculator = new PromotionDiscountCalculator(couponRepository, campaignRepository, redemptionRepository);
            var (discount, appliedCouponCode) = await calculator.CalculateDiscountAsync(
                request.BuyerUserId,
                request.OrganizationId,
                currency,
                pricedLines,
                request.CouponCode,
                isB2B: request.OrganizationId is not null,
                quantityLines,
                ct);

            // Re-allocate discount across items (MVP: proportional by line total).
            discount = Math.Clamp(discount, 0, subtotal);
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
                appliedCouponCode,
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

