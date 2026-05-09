using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.PromotionAggregate;

public sealed class CouponUsageReservation : Entity
{
    private CouponUsageReservation() { }

    public Guid OrderId { get; private set; }
    public Guid CouponId { get; private set; }
    public Guid BuyerUserId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public static CouponUsageReservation Create(Guid orderId, Guid couponId, Guid buyerUserId, DateTime expiresAtUtc)
    {
        if (orderId == Guid.Empty) throw new DomainException("OrderId is required.");
        if (couponId == Guid.Empty) throw new DomainException("CouponId is required.");
        if (buyerUserId == Guid.Empty) throw new DomainException("BuyerUserId is required.");
        if (expiresAtUtc == default) throw new DomainException("ExpiresAtUtc is required.");

        return new CouponUsageReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CouponId = couponId,
            BuyerUserId = buyerUserId,
            ExpiresAtUtc = DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc)
        };
    }
}

