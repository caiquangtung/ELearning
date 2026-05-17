using ELearning.Domain.Aggregates.CertificateAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CertificateTemplateConfiguration : IEntityTypeConfiguration<CertificateTemplate>
{
    public void Configure(EntityTypeBuilder<CertificateTemplate> builder)
    {
        builder.ToTable("certificate_templates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(t => t.HtmlTemplate).HasColumnName("html_template").IsRequired();
        builder.Property(t => t.IsDefault).HasColumnName("is_default").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(t => t.Name).IsUnique();
    }
}
