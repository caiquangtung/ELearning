namespace ELearning.Core.Abstractions;

public interface ICouponUsageReservationRepository
{
    Task<bool> TryReserveAsync(Guid couponId, Guid buyerUserId, Guid orderId, DateTime expiresAtUtc, int perBuyerMaxRedemptions, CancellationToken ct = default);
    Task ReleaseForOrderAsync(Guid orderId, CancellationToken ct = default);
}

