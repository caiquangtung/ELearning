using ELearning.Domain.Aggregates.LicensePoolAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class LicensePoolConfiguration : IEntityTypeConfiguration<LicensePool>
{
    public void Configure(EntityTypeBuilder<LicensePool> builder)
    {
        builder.ToTable("license_pools");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.TotalSeats).HasColumnName("total_seats").IsRequired();
        builder.Property(p => p.SeatPriceCents).HasColumnName("seat_price_cents").IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.ExpiresAt).HasColumnName("expires_at");

        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(p => p.Assignments)
            .WithOne()
            .HasForeignKey(a => a.LicensePoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.OrganizationId, p.Name }).IsUnique(false);
    }
}

