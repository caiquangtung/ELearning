using ELearning.Domain.Aggregates.PromotionAggregate;

namespace ELearning.Core.Abstractions;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeNormalizedAsync(string codeNormalized, CancellationToken ct = default);
}

