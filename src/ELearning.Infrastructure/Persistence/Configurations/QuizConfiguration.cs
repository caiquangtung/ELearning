using ELearning.Domain.Aggregates.QuizAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");

        builder.Property(q => q.CourseId).HasColumnName("course_id");
        builder.Property(q => q.LessonId).HasColumnName("lesson_id");

        builder.Property(q => q.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(q => q.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(q => q.TimeLimitMinutes).HasColumnName("time_limit_minutes");
        builder.Property(q => q.PassingScore).HasColumnName("passing_score");

        builder.Property(q => q.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.Property(q => q.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(q => q.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(q => !q.IsDeleted);

        builder.HasIndex(q => q.CourseId);
        builder.HasIndex(q => q.LessonId);
        builder.HasIndex(q => q.Status);

        builder.HasMany(q => q.Questions)
            .WithOne()
            .HasForeignKey(qn => qn.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
