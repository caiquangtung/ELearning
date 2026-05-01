using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.ListMyOrders;

public sealed class ListMyOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<ListMyOrdersQuery, Result<IReadOnlyList<OrderListItemDto>>>
{
    public async Task<Result<IReadOnlyList<OrderListItemDto>>> Handle(ListMyOrdersQuery request, CancellationToken ct)
    {
        var items = await orders.ListForBuyerAsync(request.BuyerUserId, request.Take, ct);
        return items
            .Select(o => new OrderListItemDto(o.Id, o.Status.ToString(), o.Currency, o.TotalCents, o.CreatedAt))
            .ToList();
    }
}

