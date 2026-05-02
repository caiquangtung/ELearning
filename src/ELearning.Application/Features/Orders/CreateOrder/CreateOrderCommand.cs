using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<CreateOrderItem> Items,
    long DiscountCents = 0)
    : IRequest<Result<OrderDto>>;

public sealed record CreateOrderItem(
    string ItemType,
    Guid ReferenceId,
    int Quantity,
    long UnitPriceCents);

