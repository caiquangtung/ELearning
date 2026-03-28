using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.RefreshTokenHash == refreshTokenHash, ct);
}
