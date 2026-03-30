using ELearning.Domain.Aggregates.CourseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasMany(c => c.Sections)
            .WithOne()
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

