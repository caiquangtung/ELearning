using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.GetOrder;

public sealed class GetOrderQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound("Order.NotFound", "Order not found."));

        return new OrderDto(
            order.Id,
            order.BuyerUserId,
            order.OrganizationId,
            order.Status.ToString(),
            order.Currency,
            order.SubtotalCents,
            order.DiscountCents,
            order.TotalCents,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(
                    i.ReferenceId,
                    i.ItemType.ToString(),
                    i.Quantity,
                    i.UnitPriceCents,
                    i.LineTotalCents,
                    i.Currency))
                .ToList());
    }
}

