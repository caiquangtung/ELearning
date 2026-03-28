using ELearning.Domain.Aggregates.OrganizationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("organization_members");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(m => m.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(m => m.DepartmentId).HasColumnName("department_id");
        builder.Property(m => m.OrgRole)
            .HasColumnName("org_role")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(m => m.JoinedAt).HasColumnName("joined_at").IsRequired();

        builder.HasIndex(m => new { m.OrganizationId, m.UserId }).IsUnique();
    }
}
