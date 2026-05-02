using ELearning.Domain.Aggregates.CommerceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(64).IsRequired();
        builder.Property(i => i.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(i => i.TotalCents).HasColumnName("total_cents").IsRequired();
        builder.Property(i => i.IssuedAt).HasColumnName("issued_at").IsRequired();

        builder.HasIndex(i => i.OrderId).IsUnique();
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
    }
}
