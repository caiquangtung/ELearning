using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiQueryEmbeddingCacheConfiguration : IEntityTypeConfiguration<AiQueryEmbeddingCache>
{
    public void Configure(EntityTypeBuilder<AiQueryEmbeddingCache> builder)
    {
        builder.ToTable("ai_query_embedding_cache");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QueryHash).HasColumnName("query_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.NormalizedQuery).HasColumnName("normalized_query").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Dimensions).HasColumnName("dimensions").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnName("embedding_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.QueryHash, x.Provider, x.Model, x.Dimensions }).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}
