using ELearning.Application.Features.Orders.Common;
using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICourseRepository courseRepository,
    ITrainingClassRepository trainingClassRepository,
    ILicensePoolRepository licensePoolRepository,
    ICheckoutReservationRepository reservationRepository,
    ICouponRepository couponRepository,
    ICampaignRepository campaignRepository,
    ICouponRedemptionRepository redemptionRepository,
    ICouponUsageReservationRepository couponUsageReservationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        try
        {
            List<(OrderItemType Type, Guid ReferenceId, int Quantity)> parsedItems;
            try
            {
                parsedItems = request.Items
                    .Select(i => (ItemType: Enum.Parse<OrderItemType>(i.ItemType, ignoreCase: true), i.ReferenceId, i.Quantity))
                    .ToList();
            }
            catch (ArgumentException)
            {
                return Result.Failure<OrderDto>(Error.Validation("Order.InvalidItemType", "Invalid ItemType."));
            }

            var utcNow = DateTime.UtcNow;
            foreach (var group in parsedItems.Where(i => i.Type == OrderItemType.TrainingClass).GroupBy(i => i.ReferenceId))
            {
                var tc = await trainingClassRepository.GetByIdAsync(group.Key, ct);
                if (tc is null)
                    throw new DomainException("Training class not found.");

                var qtyThisOrder = group.Sum(i => i.Quantity);
                var reservedElsewhere = await reservationRepository.SumActiveReservedQuantityAsync(group.Key, utcNow, ct);

                const int enrolledLearners = 0;
                if (reservedElsewhere + qtyThisOrder > tc.MaxLearners - enrolledLearners)
                    throw new DomainException("Training class seat capacity exceeded during checkout.");
            }

            var order = Order.CreateDraft(request.BuyerUserId, request.OrganizationId, request.Currency);

            var quantityLines = new List<(OrderItemType ItemType, int Quantity, long UnitPriceCents)>();
            var pricedLines = new List<(OrderItemType ItemType, long LineTotalCents)>();

            foreach (var i in parsedItems)
            {
                var priced = await GetUnitPriceAsync(i.Type, i.ReferenceId, ct);
                if (!priced.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase))
                    return Result.Failure<OrderDto>(
                        Error.Conflict(
                            "Order.CurrencyMismatch",
                            $"Item currency {priced.Currency} does not match order currency {order.Currency}."));

                order.AddItem(i.Type, i.ReferenceId, i.Quantity, priced.UnitPriceCents);
                quantityLines.Add((i.Type, i.Quantity, priced.UnitPriceCents));
                pricedLines.Add((i.Type, priced.UnitPriceCents * i.Quantity));
            }

            // Apply promotions server-side (client-supplied DiscountCents is treated as informational only).
            // Stacking + B2B volume discount rules live here to keep order totals authoritative.
            var calculator = new PromotionDiscountCalculator(couponRepository, campaignRepository, redemptionRepository);
            var (discount, appliedCouponCode) = await calculator.CalculateDiscountAsync(
                request.BuyerUserId,
                request.OrganizationId,
                order.Currency,
                pricedLines,
                request.CouponCode,
                isB2B: request.OrganizationId is not null,
                quantityLines,
                ct);

            if (discount > 0)
                order.ApplyManualDiscount(discount);
            order.SetAppliedCouponCode(appliedCouponCode);

            order.SubmitForPayment(CommerceConstants.CheckoutTimeout);

            // Atomic per-buyer coupon reservation (expires with checkout window).
            if (!string.IsNullOrWhiteSpace(appliedCouponCode))
            {
                var normalized = Domain.Aggregates.PromotionAggregate.Coupon.NormalizeCode(appliedCouponCode);
                var coupon = await couponRepository.GetByCodeNormalizedAsync(normalized, ct);
                if (coupon is null)
                    return Result.Failure<OrderDto>(Error.Validation("CouponCode", "Invalid coupon code."));

                var reserved = await couponUsageReservationRepository.TryReserveAsync(
                    coupon.Id,
                    request.BuyerUserId,
                    order.Id,
                    order.CheckoutExpiresAtUtc!.Value,
                    coupon.PerBuyerMaxRedemptions,
                    ct);

                if (!reserved)
                    return Result.Failure<OrderDto>(Error.Conflict("Coupon.UsageLimit", "Coupon usage limit reached."));
            }

            foreach (var item in order.Items.Where(it => it.ItemType == OrderItemType.TrainingClass))
            {
                reservationRepository.Add(
                    CheckoutReservation.Create(
                        order.Id,
                        item.ReferenceId,
                        item.Quantity,
                        order.CheckoutExpiresAtUtc!.Value));
            }

            orderRepository.Add(order);
            await unitOfWork.SaveChangesAsync(ct);

            return OrderDtoMapper.ToDto(order);
        }
        catch (DomainException ex)
        {
            return Result.Failure<OrderDto>(Error.Conflict("Order", ex.Message));
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
