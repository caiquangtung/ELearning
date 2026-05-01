using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.ListMyOrders;

public sealed record ListMyOrdersQuery(Guid BuyerUserId, int Take = 50)
    : IRequest<Result<IReadOnlyList<OrderListItemDto>>>;

