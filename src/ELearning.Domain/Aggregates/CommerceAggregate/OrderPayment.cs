using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CommerceAggregate;

public sealed class OrderPayment : AggregateRoot
{
    private OrderPayment() { }

    public Guid OrderId { get; private set; }
    public long AmountCents { get; private set; }
    public string Currency { get; private set; } = "USD";
    public OrderPaymentStatus Status { get; private set; }
    public string Provider { get; private set; } = "NoOp";
    public string ExternalTransactionId { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public static OrderPayment CreatePending(Guid orderId, long amountCents, string currency, string provider, string externalTransactionId)
    {
        if (orderId == Guid.Empty) throw new DomainException("OrderId is required.");
        if (amountCents <= 0) throw new DomainException("Amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (string.IsNullOrWhiteSpace(externalTransactionId)) throw new DomainException("External transaction id is required.");

        return new OrderPayment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            AmountCents = amountCents,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = OrderPaymentStatus.Pending,
            Provider = string.IsNullOrWhiteSpace(provider) ? "NoOp" : provider.Trim(),
            ExternalTransactionId = externalTransactionId.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkSucceeded()
    {
        if (Status == OrderPaymentStatus.Succeeded) return;
        Status = OrderPaymentStatus.Succeeded;
    }

    public void MarkFailed()
    {
        Status = OrderPaymentStatus.Failed;
    }
}
