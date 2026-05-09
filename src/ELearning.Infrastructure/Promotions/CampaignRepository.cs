using ELearning.Core.Abstractions;
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
}

