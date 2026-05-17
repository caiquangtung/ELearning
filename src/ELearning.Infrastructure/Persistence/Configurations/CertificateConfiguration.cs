using ELearning.Domain.Aggregates.CertificateAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("certificates");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.CourseId).HasColumnName("course_id").IsRequired();
        builder.Property(c => c.TrainingClassId).HasColumnName("training_class_id");
        builder.Property(c => c.QuizAttemptId).HasColumnName("quiz_attempt_id");

        builder.Property(c => c.CertificateNumber)
            .HasColumnName("certificate_number")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.VerificationCode)
            .HasColumnName("verification_code")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.LearnerName)
            .HasColumnName("learner_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CourseTitle)
            .HasColumnName("course_title")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(c => c.IssuedAt).HasColumnName("issued_at").IsRequired();
        builder.Property(c => c.ExpiresAt).HasColumnName("expires_at");
        builder.Property(c => c.AttendancePercent).HasColumnName("attendance_percent").HasPrecision(5, 2);
        builder.Property(c => c.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
        builder.Property(c => c.QuizPassed).HasColumnName("quiz_passed").IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(c => c.RevocationReason).HasColumnName("revocation_reason").HasMaxLength(1000);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.VerificationCode).IsUnique();
        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => new { c.UserId, c.CourseId }).IsUnique();
    }
}
