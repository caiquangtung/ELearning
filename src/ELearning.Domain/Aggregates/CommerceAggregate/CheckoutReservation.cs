using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CommerceAggregate;

/// <summary>Holds learner seats on a training class during checkout (released on pay, cancel, or expiry).</summary>
public sealed class CheckoutReservation : Entity
{
    private CheckoutReservation() { }

    public Guid OrderId { get; private set; }
    public Guid TrainingClassId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public static CheckoutReservation Create(Guid orderId, Guid trainingClassId, int quantity, DateTime expiresAtUtc)
    {
        if (orderId == Guid.Empty) throw new DomainException("OrderId is required.");
        if (trainingClassId == Guid.Empty) throw new DomainException("TrainingClassId is required.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than 0.");

        return new CheckoutReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            TrainingClassId = trainingClassId,
            Quantity = quantity,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
