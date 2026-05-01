namespace ELearning.Application.Features.Orders.Common;

public sealed record OrderItemDto(
    Guid ReferenceId,
    string ItemType,
    int Quantity,
    long UnitPriceCents,
    long LineTotalCents,
    string Currency);

public sealed record OrderDto(
    Guid Id,
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Status,
    string Currency,
    long SubtotalCents,
    long DiscountCents,
    long TotalCents,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderListItemDto(
    Guid Id,
    string Status,
    string Currency,
    long TotalCents,
    DateTime CreatedAt);

