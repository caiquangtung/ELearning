using ELearning.Core.Abstractions;
using ELearning.Core.Common;
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

    public async Task<PagedList<LicensePool>> ListByOrganizationAsync(
        Guid organizationId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var query = DbSet
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<LicensePool>.Create(items, page, pageSize, total);
    }

    public async Task<LicensePool?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(p => p.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
