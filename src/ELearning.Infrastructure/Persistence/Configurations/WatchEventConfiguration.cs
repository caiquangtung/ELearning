using ELearning.Domain.Aggregates.VideoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class WatchEventConfiguration : IEntityTypeConfiguration<WatchEvent>
{
    public void Configure(EntityTypeBuilder<WatchEvent> builder)
    {
        builder.ToTable("watch_events");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.VideoAssetId).HasColumnName("video_asset_id").IsRequired();
        builder.Property(w => w.LessonId).HasColumnName("lesson_id").IsRequired();
        builder.Property(w => w.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(w => w.LastPositionSeconds).HasColumnName("last_position_seconds").IsRequired();
        builder.Property(w => w.DurationSeconds).HasColumnName("duration_seconds").IsRequired();
        builder.Property(w => w.WatchedSeconds).HasColumnName("watched_seconds").IsRequired();
        builder.Property(w => w.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2).IsRequired();
        builder.Property(w => w.IsCompleted).HasColumnName("is_completed").IsRequired();
        builder.Property(w => w.CompletedAt).HasColumnName("completed_at");
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(w => new { w.VideoAssetId, w.UserId }).IsUnique();
        builder.HasIndex(w => new { w.UserId, w.IsCompleted });
    }
}
