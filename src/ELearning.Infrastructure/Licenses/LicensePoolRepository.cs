using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Licenses;

public sealed class LicensePoolRepository(ApplicationDbContext context)
    : GenericRepository<LicensePool>(context), ILicensePoolRepository
{
    public async Task<IReadOnlyList<LicensePool>> ListByOrganizationAsync(Guid organizationId, CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<LicensePool?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(p => p.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}

