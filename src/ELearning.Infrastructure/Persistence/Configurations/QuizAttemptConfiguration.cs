using ELearning.Domain.Aggregates.QuizAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("quiz_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.QuizId).HasColumnName("quiz_id").IsRequired();
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(a => a.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(a => a.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(a => a.TotalScore).HasColumnName("total_score");

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => new { a.QuizId, a.UserId });
        builder.HasIndex(a => a.Status);

        builder.HasMany(a => a.Answers)
            .WithOne()
            .HasForeignKey(ans => ans.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
