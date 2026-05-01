using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
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
                order.AddItem(type, i.ReferenceId, i.Quantity, i.UnitPriceCents);
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

