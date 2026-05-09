using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.PromotionAggregate;

public sealed class CouponRedemption : Entity
{
    private CouponRedemption() { }

    public Guid CouponId { get; private set; }
    public Guid BuyerUserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public DateTime RedeemedAtUtc { get; private set; }

    public static CouponRedemption Record(Guid couponId, Guid buyerUserId, Guid? orderId, DateTime redeemedAtUtc)
    {
        if (couponId == Guid.Empty) throw new DomainException("CouponId is required.");
        if (buyerUserId == Guid.Empty) throw new DomainException("BuyerUserId is required.");
        if (redeemedAtUtc == default) throw new DomainException("RedeemedAtUtc is required.");

        return new CouponRedemption
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            BuyerUserId = buyerUserId,
            OrderId = orderId,
            RedeemedAtUtc = DateTime.SpecifyKind(redeemedAtUtc, DateTimeKind.Utc)
        };
    }
}

