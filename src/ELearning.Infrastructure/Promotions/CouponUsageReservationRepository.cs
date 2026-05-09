using ELearning.Core.Abstractions;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Promotions;

public sealed class CouponUsageReservationRepository(ApplicationDbContext context) : ICouponUsageReservationRepository
{
    public async Task<bool> TryReserveAsync(
        Guid couponId,
        Guid buyerUserId,
        Guid orderId,
        DateTime expiresAtUtc,
        int perBuyerMaxRedemptions,
        CancellationToken ct = default)
    {
        // Atomic single statement: reserve iff (active_reservations + redemptions) < max
        var sql = """
                  INSERT INTO coupon_usage_reservations (id, order_id, coupon_id, buyer_user_id, expires_at_utc)
                  SELECT {0}, {1}, {2}, {3}, {4}
                  WHERE (
                      (SELECT COUNT(*) FROM coupon_usage_reservations
                       WHERE coupon_id = {2} AND buyer_user_id = {3} AND expires_at_utc > NOW())
                    + (SELECT COUNT(*) FROM coupon_redemptions
                       WHERE coupon_id = {2} AND buyer_user_id = {3})
                  ) < {5};
                  """;

        var affected = await context.Database.ExecuteSqlRawAsync(
            sql,
            parameters: [Guid.NewGuid(), orderId, couponId, buyerUserId, expiresAtUtc, perBuyerMaxRedemptions],
            cancellationToken: ct);

        return affected > 0;
    }

    public Task ReleaseForOrderAsync(Guid orderId, CancellationToken ct = default) =>
        context.CouponUsageReservations.Where(r => r.OrderId == orderId).ExecuteDeleteAsync(ct);
}

