using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiKnowledgeChunkConfiguration : IEntityTypeConfiguration<AiKnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeChunk> builder)
    {
        builder.ToTable("ai_knowledge_chunks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CourseId).HasColumnName("course_id").IsRequired();
        builder.Property(x => x.SectionId).HasColumnName("section_id");
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.CourseTitle).HasColumnName("course_title").HasMaxLength(300).IsRequired();
        builder.Property(x => x.SectionTitle).HasColumnName("section_title").HasMaxLength(300);
        builder.Property(x => x.LessonTitle).HasColumnName("lesson_title").HasMaxLength(300);
        builder.Property(x => x.ChunkIndex).HasColumnName("chunk_index").IsRequired();
        builder.Property(x => x.ContentHash).HasColumnName("content_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Text).HasColumnName("text").HasColumnType("text").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnName("embedding_json").HasColumnType("jsonb").IsRequired();
        builder.Ignore(x => x.EmbeddingVector);
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.CourseId);
        builder.HasIndex(x => x.LessonId);
        builder.HasIndex(x => x.ContentHash).IsUnique();
        builder.HasIndex(x => new { x.CourseId, x.SourceType, x.ChunkIndex });
    }
}
