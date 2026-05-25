using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Promotions;

public sealed class CampaignRepository(ApplicationDbContext context)
    : GenericRepository<Campaign>(context), ICampaignRepository
{
    public async Task<Campaign?> GetByIdWithRulesAndCouponsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(c => c.Rules)
            .Include(c => c.Coupons)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Campaign>> ListAsync(Guid? organizationId, int take, CancellationToken ct = default)
    {
        take = take is <= 0 or > 200 ? 50 : take;
        var query = DbSet.AsNoTracking();
        query = organizationId is null
            ? query.Where(c => c.OrganizationId == null)
            : query.Where(c => c.OrganizationId == organizationId);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<PagedList<Campaign>> ListAsync(
        Guid? organizationId,
        bool includeGlobal,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var query = DbSet.AsNoTracking().AsQueryable();

        query = organizationId is null
            ? query.Where(c => includeGlobal && c.OrganizationId == null)
            : query.Where(c => c.OrganizationId == organizationId || (includeGlobal && c.OrganizationId == null));

        query = query.OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Campaign>.Create(items, page, pageSize, total);
    }
}
