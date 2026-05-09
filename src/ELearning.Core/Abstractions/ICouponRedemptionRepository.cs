namespace ELearning.Core.Abstractions;

public interface ICouponRedemptionRepository
{
    Task<IReadOnlyList<Domain.Aggregates.PromotionAggregate.CouponRedemption>> FindAsync(
        System.Linq.Expressions.Expression<Func<Domain.Aggregates.PromotionAggregate.CouponRedemption, bool>> predicate,
        CancellationToken ct = default);
    Task<int> CountForBuyerAsync(Guid couponId, Guid buyerUserId, CancellationToken ct = default);
    void AddRedemption(Guid couponId, Guid buyerUserId, Guid? orderId, DateTime redeemedAtUtc);
}

