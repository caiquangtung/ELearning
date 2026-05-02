namespace ELearning.Core.Abstractions;

public interface ICouponRedemptionRepository
{
    Task<int> CountForBuyerAsync(Guid couponId, Guid buyerUserId, CancellationToken ct = default);
    void AddRedemption(Guid couponId, Guid buyerUserId, Guid? orderId, DateTime redeemedAtUtc);
}

