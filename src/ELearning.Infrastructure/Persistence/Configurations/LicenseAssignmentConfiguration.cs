using ELearning.Domain.Aggregates.LicensePoolAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class LicenseAssignmentConfiguration : IEntityTypeConfiguration<LicenseAssignment>
{
    public void Configure(EntityTypeBuilder<LicenseAssignment> builder)
    {
        builder.ToTable("license_assignments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.LicensePoolId).HasColumnName("license_pool_id").IsRequired();
        builder.Property(a => a.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(a => a.AssignedAt).HasColumnName("assigned_at").IsRequired();
        builder.Property(a => a.RevokedAt).HasColumnName("revoked_at");

        builder.HasIndex(a => new { a.LicensePoolId, a.UserId }).IsUnique();
        builder.HasIndex(a => new { a.OrganizationId, a.UserId });
    }
}

