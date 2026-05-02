using ELearning.Domain.Aggregates.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Scope).HasColumnName("scope").HasConversion<string>().IsRequired();
        builder.Property(c => c.OrganizationId).HasColumnName("organization_id");
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>().IsRequired();

        builder.Property(c => c.StartUtc).HasColumnName("start_utc").IsRequired();
        builder.Property(c => c.EndUtc).HasColumnName("end_utc");

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(c => c.Rules)
            .WithOne()
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Coupons)
            .WithOne()
            .HasForeignKey(cp => cp.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.OrganizationId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Scope);
    }
}

