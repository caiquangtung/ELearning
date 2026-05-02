using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Promotions;

public sealed class CouponRepository(ApplicationDbContext context)
    : GenericRepository<Coupon>(context), ICouponRepository
{
    public async Task<Coupon?> GetByCodeNormalizedAsync(string codeNormalized, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.CodeNormalized == codeNormalized, ct);
}

