using ELearning.Domain.Aggregates.QuizAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuestionAnswerConfiguration : IEntityTypeConfiguration<QuestionAnswer>
{
    public void Configure(EntityTypeBuilder<QuestionAnswer> builder)
    {
        builder.ToTable("quiz_question_answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.QuizAttemptId).HasColumnName("quiz_attempt_id").IsRequired();
        builder.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(a => a.SelectedOptionId).HasColumnName("selected_option_id");

        builder.Property(a => a.TextAnswer)
            .HasColumnName("text_answer")
            .HasColumnType("text");

        builder.Property(a => a.Score).HasColumnName("score");
        builder.Property(a => a.IsCorrect).HasColumnName("is_correct");
        builder.Property(a => a.GradedAt).HasColumnName("graded_at");
        builder.Property(a => a.GradedBy)
            .HasColumnName("graded_by")
            .HasMaxLength(256);

        builder.HasIndex(a => new { a.QuizAttemptId, a.QuestionId }).IsUnique();
    }
}
