using ELearning.Domain.Aggregates.CourseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class ContentAssetConfiguration : IEntityTypeConfiguration<ContentAsset>
{
    public void Configure(EntityTypeBuilder<ContentAsset> builder)
    {
        builder.ToTable("content_assets");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.LessonId).HasColumnName("lesson_id").IsRequired();

        builder.Property(a => a.AssetType)
            .HasColumnName("asset_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.SizeBytes).HasColumnName("size_bytes").IsRequired();

        builder.Property(a => a.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(a => a.Url)
            .HasColumnName("url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at").IsRequired();

        builder.Property(a => a.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasIndex(a => a.LessonId);
        builder.HasIndex(a => a.StorageKey).IsUnique();
    }
}

