using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CommerceAggregate;

public sealed class Invoice : AggregateRoot
{
    private Invoice() { }

    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public string Currency { get; private set; } = "USD";
    public long TotalCents { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public static Invoice Issue(Guid orderId, string invoiceNumber, string currency, long totalCents)
    {
        if (orderId == Guid.Empty) throw new DomainException("OrderId is required.");
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new DomainException("Invoice number is required.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (totalCents <= 0) throw new DomainException("Total must be greater than 0.");

        return new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            InvoiceNumber = invoiceNumber.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            TotalCents = totalCents,
            IssuedAt = DateTime.UtcNow
        };
    }
}
