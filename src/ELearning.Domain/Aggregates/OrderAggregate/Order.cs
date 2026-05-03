using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.OrderAggregate;

public sealed class Order : AuditableAggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order() { }

    public Guid BuyerUserId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public OrderStatus Status { get; private set; }

    public string Currency { get; private set; } = "USD";
    public long SubtotalCents { get; private set; }
    public long DiscountCents { get; private set; }
    public long TotalCents { get; private set; }
    public string? AppliedCouponCode { get; private set; }

    /// <summary>When status is PendingPayment, checkout must complete before this instant (UTC).</summary>
    public DateTime? CheckoutExpiresAtUtc { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public static Order CreateDraft(Guid buyerUserId, Guid? organizationId, string currency)
    {
        if (buyerUserId == Guid.Empty) throw new DomainException("BuyerUserId is required.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");

        return new Order
        {
            Id = Guid.NewGuid(),
            BuyerUserId = buyerUserId,
            OrganizationId = organizationId,
            Status = OrderStatus.Draft,
            Currency = currency.Trim().ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(OrderItemType itemType, Guid referenceId, int quantity, long unitPriceCents)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Can only add items to a draft order.");

        var item = OrderItem.Create(itemType, referenceId, quantity, unitPriceCents, Currency);
        item.SetOrderId(Id);
        _items.Add(item);
        RecalculateTotals();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyManualDiscount(long discountCents)
    {
        if (discountCents < 0) throw new DomainException("Discount must be non-negative.");
        if (discountCents > SubtotalCents) throw new DomainException("Discount cannot exceed subtotal.");

        DiscountCents = discountCents;
        RecalculateTotals();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAppliedCouponCode(string? couponCode)
    {
        AppliedCouponCode = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubmitForPayment(TimeSpan checkoutTimeout)
    {
        if (Status != OrderStatus.Draft) throw new DomainException("Order must be draft.");
        if (_items.Count == 0) throw new DomainException("Order must have at least one item.");
        if (TotalCents <= 0) throw new DomainException("Order total must be greater than 0.");
        if (checkoutTimeout <= TimeSpan.Zero) throw new DomainException("Checkout timeout must be positive.");

        CheckoutExpiresAtUtc = DateTime.UtcNow.Add(checkoutTimeout);
        Status = OrderStatus.PendingPayment;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>If checkout window elapsed while pending payment, cancel and release reservations externally.</summary>
    public bool TryExpireCheckout(DateTime utcNow)
    {
        if (Status != OrderStatus.PendingPayment) return false;
        if (CheckoutExpiresAtUtc is null || utcNow <= CheckoutExpiresAtUtc.Value) return false;

        Status = OrderStatus.Cancelled;
        UpdatedAt = utcNow;
        return true;
    }

    public void MarkPaid()
    {
        if (Status == OrderStatus.Paid) return;
        if (Status != OrderStatus.PendingPayment) throw new DomainException("Order must be pending payment.");
        Status = OrderStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Paid) throw new DomainException("Paid orders cannot be cancelled.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("Cancel reason is required.");
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        SubtotalCents = _items.Sum(i => i.LineTotalCents);
        TotalCents = Math.Max(0, SubtotalCents - DiscountCents);
    }
}

