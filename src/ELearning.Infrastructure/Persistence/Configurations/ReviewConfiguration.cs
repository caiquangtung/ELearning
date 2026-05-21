using ELearning.Domain.Aggregates.ReviewAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.CourseId).HasColumnName("course_id").IsRequired();
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.Rating).HasColumnName("rating").IsRequired();
        builder.Property(r => r.Comment).HasColumnName("comment").HasMaxLength(4000).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.SubmittedAt).HasColumnName("submitted_at").IsRequired();
        builder.Property(r => r.ModeratedAt).HasColumnName("moderated_at");
        builder.Property(r => r.ModeratedByUserId).HasColumnName("moderated_by_user_id");
        builder.Property(r => r.ModerationReason).HasColumnName("moderation_reason").HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => new { r.CourseId, r.UserId }).IsUnique();
        builder.HasIndex(r => new { r.CourseId, r.Status });
    }
}
