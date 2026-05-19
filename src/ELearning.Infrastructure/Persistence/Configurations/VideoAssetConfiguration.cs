using ELearning.Domain.Aggregates.VideoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.ToTable("video_assets");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.LessonId).HasColumnName("lesson_id").IsRequired();
        builder.Property(v => v.FileName).HasColumnName("file_name").HasMaxLength(512).IsRequired();
        builder.Property(v => v.ContentType).HasColumnName("content_type").HasMaxLength(256).IsRequired();
        builder.Property(v => v.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(v => v.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
        builder.Property(v => v.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        builder.Property(v => v.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(v => v.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(v => v.LessonId);
        builder.HasIndex(v => v.StorageKey).IsUnique();
    }
}
