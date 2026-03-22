using ELearning.Core.Abstractions;
using ELearning.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService)
    : DbContext(options), IUnitOfWork
{
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
