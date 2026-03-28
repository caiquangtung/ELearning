using ELearning.Core.Abstractions;
using ELearning.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ELearning.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("ELearning__ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=elearning_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(cs)
            .Options;

        ICurrentUserService currentUser = new NullCurrentUserService();
        return new ApplicationDbContext(options, currentUser);
    }
}
