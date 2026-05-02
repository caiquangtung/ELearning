using ELearning.Domain.Aggregates.CommerceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CheckoutReservationConfiguration : IEntityTypeConfiguration<CheckoutReservation>
{
    public void Configure(EntityTypeBuilder<CheckoutReservation> builder)
    {
        builder.ToTable("checkout_reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(r => r.TrainingClassId).HasColumnName("training_class_id").IsRequired();
        builder.Property(r => r.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(r => r.ExpiresAtUtc).HasColumnName("expires_at").IsRequired();

        builder.HasIndex(r => new { r.OrderId, r.TrainingClassId });
        builder.HasIndex(r => r.TrainingClassId);
    }
}
