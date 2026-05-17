using ELearning.Domain.Aggregates.QuizAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");

        builder.Property(q => q.QuizId).HasColumnName("quiz_id").IsRequired();

        builder.Property(q => q.Text)
            .HasColumnName("text")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(q => q.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(q => q.Points).HasColumnName("points").IsRequired();
        builder.Property(q => q.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.Property(q => q.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(q => q.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(q => !q.IsDeleted);

        builder.HasIndex(q => new { q.QuizId, q.SortOrder }).IsUnique();

        builder.HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
