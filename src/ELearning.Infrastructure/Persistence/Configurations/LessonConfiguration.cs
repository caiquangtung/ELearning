using ELearning.Domain.Aggregates.CourseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("course_lessons");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.SectionId).HasColumnName("section_id").IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.Property(l => l.Content).HasColumnName("content");

        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        builder.Property(l => l.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.HasIndex(l => new { l.SectionId, l.SortOrder }).IsUnique();

        builder.HasMany(l => l.Assets)
            .WithOne()
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

