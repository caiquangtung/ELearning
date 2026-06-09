using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiKnowledgeReindexJobConfiguration : IEntityTypeConfiguration<AiKnowledgeReindexJob>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeReindexJob> builder)
    {
        builder.ToTable("ai_knowledge_reindex_jobs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CourseId).HasColumnName("course_id");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.IndexedCourses).HasColumnName("indexed_courses").IsRequired();
        builder.Property(x => x.IndexedChunks).HasColumnName("indexed_chunks").IsRequired();
        builder.Property(x => x.DeletedStaleChunks).HasColumnName("deleted_stale_chunks").IsRequired();
        builder.Property(x => x.Error).HasColumnName("error").HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.CourseId);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => x.CreatedAt);
    }
}
