using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class ClassInstructorConfiguration : IEntityTypeConfiguration<ClassInstructor>
{
    public void Configure(EntityTypeBuilder<ClassInstructor> builder)
    {
        builder.ToTable("class_instructors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingClassId).HasColumnName("training_class_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.AssignedAt).HasColumnName("assigned_at").IsRequired();

        builder.HasIndex(x => new { x.TrainingClassId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
