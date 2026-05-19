using ELearning.Domain.Aggregates.NotificationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.SenderUserId).HasColumnName("sender_user_id").IsRequired();
        builder.Property(m => m.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Body).HasColumnName("body").HasMaxLength(4000).IsRequired();
        builder.Property(m => m.Scope).HasColumnName("scope").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(m => m.OrganizationId).HasColumnName("organization_id");
        builder.Property(m => m.CourseId).HasColumnName("course_id");
        builder.Property(m => m.TrainingClassId).HasColumnName("training_class_id");
        builder.Property(m => m.RecipientCount).HasColumnName("recipient_count").IsRequired();
        builder.Property(m => m.SentAt).HasColumnName("sent_at").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.SenderUserId);
        builder.HasIndex(m => m.OrganizationId);
        builder.HasIndex(m => m.CourseId);
        builder.HasIndex(m => m.TrainingClassId);
    }
}
