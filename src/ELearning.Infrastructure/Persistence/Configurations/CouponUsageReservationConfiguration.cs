using ELearning.Domain.Aggregates.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CouponUsageReservationConfiguration : IEntityTypeConfiguration<CouponUsageReservation>
{
    public void Configure(EntityTypeBuilder<CouponUsageReservation> builder)
    {
        builder.ToTable("coupon_usage_reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(r => r.CouponId).HasColumnName("coupon_id").IsRequired();
        builder.Property(r => r.BuyerUserId).HasColumnName("buyer_user_id").IsRequired();
        builder.Property(r => r.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();

        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => new { r.CouponId, r.BuyerUserId });
    }
}

