using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICourseRepository courseRepository,
    ITrainingClassRepository trainingClassRepository,
    ILicensePoolRepository licensePoolRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        try
        {
            var order = Order.CreateDraft(request.BuyerUserId, request.OrganizationId, request.Currency);

            foreach (var i in request.Items)
            {
                var type = Enum.Parse<OrderItemType>(i.ItemType, ignoreCase: true);
                var priced = await GetUnitPriceAsync(type, i.ReferenceId, ct);
                if (!priced.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase))
                    return Result.Failure<OrderDto>(Error.Conflict("Order.CurrencyMismatch", $"Item currency {priced.Currency} does not match order currency {order.Currency}."));

                order.AddItem(type, i.ReferenceId, i.Quantity, priced.UnitPriceCents);
            }

            if (request.DiscountCents > 0)
                order.ApplyManualDiscount(request.DiscountCents);

            order.SubmitForPayment();

            orderRepository.Add(order);
            await unitOfWork.SaveChangesAsync(ct);

            return ToDto(order);
        }
        catch (DomainException ex)
        {
            return Result.Failure<OrderDto>(Error.Conflict("Order", ex.Message));
        }
        catch (ArgumentException)
        {
            return Result.Failure<OrderDto>(Error.Validation("Order.InvalidItemType", "Invalid ItemType."));
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

    private static OrderDto ToDto(Order o) =>
        new(
            o.Id,
            o.BuyerUserId,
            o.OrganizationId,
            o.Status.ToString(),
            o.Currency,
            o.SubtotalCents,
            o.DiscountCents,
            o.TotalCents,
            o.CreatedAt,
            o.UpdatedAt,
            o.Items.Select(i => new OrderItemDto(
                    i.ReferenceId,
                    i.ItemType.ToString(),
                    i.Quantity,
                    i.UnitPriceCents,
                    i.LineTotalCents,
                    i.Currency))
                .ToList());
}

