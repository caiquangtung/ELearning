using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Identity;

public class OrganizationRepository(ApplicationDbContext context)
    : GenericRepository<Organization>(context), IOrganizationRepository
{
    public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await DbSet
            .Include(o => o.Departments)
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Slug == slug.ToLowerInvariant(), ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await DbSet.AnyAsync(o => o.Slug == slug.ToLowerInvariant(), ct);

    public async Task<OrganizationMember?> FindMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken ct = default) =>
        await DbContext.Set<OrganizationMember>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.UserId == userId,
                ct);

    public async Task<Organization?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(o => o.Departments)
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Organization>> GetOrganizationsForUserAsync(
        Guid userId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(o => o.Members.Any(m => m.UserId == userId))
            .ToListAsync(ct);
}
