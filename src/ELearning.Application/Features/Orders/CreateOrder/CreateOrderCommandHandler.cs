using ELearning.Application.Features.Orders.Common;
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

            foreach (var i in parsedItems)
            {
                var priced = await GetUnitPriceAsync(i.Type, i.ReferenceId, ct);
                if (!priced.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase))
                    return Result.Failure<OrderDto>(
                        Error.Conflict(
                            "Order.CurrencyMismatch",
                            $"Item currency {priced.Currency} does not match order currency {order.Currency}."));

                order.AddItem(i.Type, i.ReferenceId, i.Quantity, priced.UnitPriceCents);
            }

            if (request.DiscountCents > 0)
                order.ApplyManualDiscount(request.DiscountCents);

            order.SubmitForPayment(CommerceConstants.CheckoutTimeout);

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
