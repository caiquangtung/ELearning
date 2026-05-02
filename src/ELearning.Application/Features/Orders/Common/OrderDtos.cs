using ELearning.Domain.Aggregates.OrderAggregate;

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
    DateTime? CheckoutExpiresAtUtc,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderListItemDto(
    Guid Id,
    string Status,
    string Currency,
    long TotalCents,
    DateTime CreatedAt);

public sealed record InvoiceDto(
    Guid Id,
    Guid OrderId,
    string InvoiceNumber,
    string Currency,
    long TotalCents,
    DateTime IssuedAt);

public static class OrderDtoMapper
{
    public static OrderDto ToDto(Order order) =>
        new(
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
            order.CheckoutExpiresAtUtc,
            order.Items.Select(i => new OrderItemDto(
                    i.ReferenceId,
                    i.ItemType.ToString(),
                    i.Quantity,
                    i.UnitPriceCents,
                    i.LineTotalCents,
                    i.Currency))
                .ToList());
}

