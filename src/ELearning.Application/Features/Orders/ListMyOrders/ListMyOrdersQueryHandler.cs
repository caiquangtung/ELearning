using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.ListMyOrders;

public sealed class ListMyOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<ListMyOrdersQuery, Result<PagedList<OrderListItemDto>>>
{
    public async Task<Result<PagedList<OrderListItemDto>>> Handle(ListMyOrdersQuery request, CancellationToken ct)
    {
        var page = await orders.ListForBuyerAsync(request.BuyerUserId, request.Page, request.PageSize, ct);
        var items = page.Items
            .Select(o => new OrderListItemDto(o.Id, o.Status.ToString(), o.Currency, o.TotalCents, o.CreatedAt))
            .ToList();

        return PagedList<OrderListItemDto>.Create(items, page.Page, page.PageSize, page.TotalCount);
    }
}
