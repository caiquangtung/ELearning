using ELearning.Domain.Aggregates.QuizAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("quiz_question_options");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.QuestionId).HasColumnName("question_id").IsRequired();

        builder.Property(o => o.Text)
            .HasColumnName("text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(o => o.IsCorrect).HasColumnName("is_correct").IsRequired();
        builder.Property(o => o.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.Property(o => o.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(o => o.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasIndex(o => new { o.QuestionId, o.SortOrder }).IsUnique();
    }
}
