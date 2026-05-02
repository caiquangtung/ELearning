using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Promotions;

public sealed class CouponRedemptionRepository(ApplicationDbContext context) : ICouponRedemptionRepository
{
    public async Task<int> CountForBuyerAsync(Guid couponId, Guid buyerUserId, CancellationToken ct = default) =>
        await context.CouponRedemptions.CountAsync(r => r.CouponId == couponId && r.BuyerUserId == buyerUserId, ct);

    public void AddRedemption(Guid couponId, Guid buyerUserId, Guid? orderId, DateTime redeemedAtUtc) =>
        context.CouponRedemptions.Add(CouponRedemption.Record(couponId, buyerUserId, orderId, redeemedAtUtc));
}

