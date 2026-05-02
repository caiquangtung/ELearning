using ELearning.Domain.Aggregates.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.CampaignId).HasColumnName("campaign_id").IsRequired();
        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        builder.Property(c => c.CodeNormalized).HasColumnName("code_normalized").HasMaxLength(64).IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(c => c.ExpiresUtc).HasColumnName("expires_utc");
        builder.Property(c => c.PerBuyerMaxRedemptions).HasColumnName("per_buyer_max_redemptions").IsRequired();

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.CampaignId);
        builder.HasIndex(c => c.CodeNormalized).IsUnique();
        builder.HasIndex(c => c.Status);
    }
}

