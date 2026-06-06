using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("ai_chat_messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(x => x.CitationsJson).HasColumnName("citations_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(80);
        builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(120);
        builder.Property(x => x.PromptVersion).HasColumnName("prompt_version").HasMaxLength(120);
        builder.Property(x => x.Confidence).HasColumnName("confidence").HasPrecision(5, 4);
        builder.Property(x => x.UsedContext).HasColumnName("used_context").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => new { x.SessionId, x.CreatedAt });
    }
}
