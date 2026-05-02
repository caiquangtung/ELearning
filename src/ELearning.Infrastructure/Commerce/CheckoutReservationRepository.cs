using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Commerce;

public sealed class CheckoutReservationRepository(ApplicationDbContext context) : ICheckoutReservationRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<int> SumActiveReservedQuantityAsync(Guid trainingClassId, DateTime utcNow, CancellationToken ct = default)
    {
        var query =
            from cr in _context.Set<CheckoutReservation>()
            join o in _context.Set<Order>() on cr.OrderId equals o.Id
            where cr.TrainingClassId == trainingClassId
                  && o.Status == OrderStatus.PendingPayment
                  && cr.ExpiresAtUtc > utcNow
            select cr.Quantity;

        return await query.SumAsync(ct);
    }

    public void Add(CheckoutReservation reservation) =>
        _context.Set<CheckoutReservation>().Add(reservation);

    public Task ReleaseForOrderAsync(Guid orderId, CancellationToken ct = default) =>
        _context.Set<CheckoutReservation>().Where(r => r.OrderId == orderId).ExecuteDeleteAsync(ct);
}
