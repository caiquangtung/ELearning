using ELearning.Domain.Aggregates.CommerceAggregate;

namespace ELearning.Core.Abstractions;

public interface ICheckoutReservationRepository
{
    /// <summary>Sum of quantities held by other pending checkouts not yet expired.</summary>
    Task<int> SumActiveReservedQuantityAsync(Guid trainingClassId, DateTime utcNow, CancellationToken ct = default);

    void Add(CheckoutReservation reservation);

    Task ReleaseForOrderAsync(Guid orderId, CancellationToken ct = default);
}
