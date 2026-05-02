using ELearning.Domain.Aggregates.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("coupon_redemptions");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.CouponId).HasColumnName("coupon_id").IsRequired();
        builder.Property(r => r.BuyerUserId).HasColumnName("buyer_user_id").IsRequired();
        builder.Property(r => r.OrderId).HasColumnName("order_id");
        builder.Property(r => r.RedeemedAtUtc).HasColumnName("redeemed_at_utc").IsRequired();

        builder.HasIndex(r => r.CouponId);
        builder.HasIndex(r => new { r.CouponId, r.BuyerUserId });
        builder.HasIndex(r => r.OrderId);
    }
}

