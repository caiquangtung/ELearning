using ELearning.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.BuyerUserId).HasColumnName("buyer_user_id").IsRequired();
        builder.Property(o => o.OrganizationId).HasColumnName("organization_id");
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().IsRequired();

        builder.Property(o => o.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(o => o.SubtotalCents).HasColumnName("subtotal_cents").IsRequired();
        builder.Property(o => o.DiscountCents).HasColumnName("discount_cents").IsRequired();
        builder.Property(o => o.TotalCents).HasColumnName("total_cents").IsRequired();

        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.BuyerUserId);
        builder.HasIndex(o => o.OrganizationId);
    }
}

