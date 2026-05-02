using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.OrderAggregate;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    private OrderItem(OrderItemType itemType, Guid referenceId, int quantity, long unitPriceCents, string currency)
    {
        if (referenceId == Guid.Empty) throw new DomainException("ReferenceId is required.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than 0.");
        if (unitPriceCents < 0) throw new DomainException("Unit price must be non-negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");

        Id = Guid.NewGuid();
        ItemType = itemType;
        ReferenceId = referenceId;
        Quantity = quantity;
        UnitPriceCents = unitPriceCents;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public Guid OrderId { get; private set; }
    public OrderItemType ItemType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public int Quantity { get; private set; }
    public long UnitPriceCents { get; private set; }
    public string Currency { get; private set; } = default!;

    public long LineTotalCents => UnitPriceCents * Quantity;

    internal static OrderItem Create(OrderItemType itemType, Guid referenceId, int quantity, long unitPriceCents, string currency) =>
        new(itemType, referenceId, quantity, unitPriceCents, currency);

    internal void SetOrderId(Guid orderId) => OrderId = orderId;
}

