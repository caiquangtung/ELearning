using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiRequestLogConfiguration : IEntityTypeConfiguration<AiRequestLog>
{
    public void Configure(EntityTypeBuilder<AiRequestLog> builder)
    {
        builder.ToTable("ai_request_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.Feature).HasColumnName("feature").HasMaxLength(100).IsRequired();
        builder.Property(l => l.Provider).HasColumnName("provider").HasMaxLength(80).IsRequired();
        builder.Property(l => l.Model).HasColumnName("model").HasMaxLength(120).IsRequired();
        builder.Property(l => l.PromptVersion).HasColumnName("prompt_version").HasMaxLength(120).IsRequired();
        builder.Property(l => l.InputHash).HasColumnName("input_hash").HasMaxLength(128).IsRequired();
        builder.Property(l => l.TokenEstimate).HasColumnName("token_estimate");
        builder.Property(l => l.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(l => new { l.Feature, l.CreatedAt });
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.InputHash);
    }
}
