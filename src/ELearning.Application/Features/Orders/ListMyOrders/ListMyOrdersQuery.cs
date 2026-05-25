using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.ListMyOrders;

public sealed record ListMyOrdersQuery(Guid BuyerUserId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedList<OrderListItemDto>>>;
