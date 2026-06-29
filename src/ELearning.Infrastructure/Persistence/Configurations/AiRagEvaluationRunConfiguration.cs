using ELearning.Domain.Aggregates.AiAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class AiRagEvaluationRunConfiguration : IEntityTypeConfiguration<AiRagEvaluationRun>
{
    public void Configure(EntityTypeBuilder<AiRagEvaluationRun> builder)
    {
        builder.ToTable("ai_rag_evaluation_runs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
        builder.Property(x => x.DatasetVersion).HasColumnName("dataset_version").HasMaxLength(80).IsRequired();
        builder.Property(x => x.TotalCases).HasColumnName("total_cases").IsRequired();
        builder.Property(x => x.PassedCases).HasColumnName("passed_cases").IsRequired();
        builder.Property(x => x.RetrievalHitRate).HasColumnName("retrieval_hit_rate").HasPrecision(6, 4).IsRequired();
        builder.Property(x => x.CitationValidityRate).HasColumnName("citation_validity_rate").HasPrecision(6, 4).IsRequired();
        builder.Property(x => x.RefusalAccuracyRate).HasColumnName("refusal_accuracy_rate").HasPrecision(6, 4).IsRequired();
        builder.Property(x => x.GroundednessRate).HasColumnName("groundedness_rate").HasPrecision(6, 4).IsRequired();
        builder.Property(x => x.Error).HasColumnName("error").HasMaxLength(2000);
        builder.Property(x => x.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
