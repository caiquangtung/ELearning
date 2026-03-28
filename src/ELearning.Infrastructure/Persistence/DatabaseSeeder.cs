using ELearning.Core.Abstractions;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ELearning.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    /// <summary>Applies migrations in all environments. Seeds the default admin user only in Development when the database is empty.</summary>
    public static async Task MigrateAndSeedAsync(this IHost host, CancellationToken ct = default)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        await db.Database.MigrateAsync(ct);

        if (!env.IsDevelopment())
            return;

        if (await db.Set<User>().AnyAsync(ct))
            return;

        var email = config["Seed:AdminEmail"] ?? "admin@localhost.local";
        var password = config["Seed:AdminPassword"] ?? "ChangeMe123!";
        var hash = hasher.Hash(password);

        var admin = User.Create(email, hash, "System", "Admin", Roles.Admin);
        db.Add(admin);
        await db.SaveChangesAsync(ct);
    }
}
