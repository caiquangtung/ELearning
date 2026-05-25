using ELearning.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(i => i.ItemType).HasColumnName("item_type").HasConversion<string>().IsRequired();
        builder.Property(i => i.ReferenceId).HasColumnName("reference_id").IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(i => i.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();
        builder.Property(i => i.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();

        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => new { i.ItemType, i.ReferenceId });
    }
}
