using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Domain.Aggregates.CertificateAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Aggregates.UserAggregate;
using ELearning.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<LicensePool> LicensePools => Set<LicensePool>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<CheckoutReservation> CheckoutReservations => Set<CheckoutReservation>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<CouponUsageReservation> CouponUsageReservations => Set<CouponUsageReservation>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<CertificateTemplate> CertificateTemplates => Set<CertificateTemplate>();
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;
        var userId = currentUserService.UserId?.ToString() ?? "system";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                TrySetProperty(entry.Entity, "CreatedAt", now);
                TrySetProperty(entry.Entity, "CreatedBy", userId);
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                TrySetProperty(entry.Entity, "UpdatedAt", now);
                TrySetProperty(entry.Entity, "UpdatedBy", userId);
            }
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                TrySetProperty(entry.Entity, "IsDeleted", true);
                TrySetProperty(entry.Entity, "DeletedAt", now);
            }
        }
    }

    private static void TrySetProperty(object entity, string property, object value)
    {
        var prop = entity.GetType().GetProperty(property);
        prop?.SetValue(entity, value);
    }
}
