using ELearning.Domain.Aggregates.TrainingClassAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("class_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingClassId).HasColumnName("training_class_id").IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SessionType)
            .HasColumnName("session_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.StartUtc).HasColumnName("start_utc").IsRequired();
        builder.Property(x => x.EndUtc).HasColumnName("end_utc").IsRequired();

        builder.Property(x => x.Location).HasColumnName("location").HasMaxLength(500);
        builder.Property(x => x.ZoomMeetingId).HasColumnName("zoom_meeting_id").HasMaxLength(200);
        builder.Property(x => x.ZoomJoinUrl).HasColumnName("zoom_join_url").HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.TrainingClassId, x.StartUtc });
        builder.HasIndex(x => x.StartUtc);
    }
}
