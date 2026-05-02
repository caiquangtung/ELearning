using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrderAggregate;
using MediatR;

namespace ELearning.Application.Features.Orders.GetOrder;

public sealed class GetOrderQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound(nameof(Order), request.OrderId));

        return OrderDtoMapper.ToDto(order);
    }
}

