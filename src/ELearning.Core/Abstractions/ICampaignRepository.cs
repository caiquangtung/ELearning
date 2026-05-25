using ELearning.Core.Common;
using ELearning.Domain.Aggregates.PromotionAggregate;

namespace ELearning.Core.Abstractions;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<Campaign?> GetByIdWithRulesAndCouponsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Campaign>> ListAsync(Guid? organizationId, int take, CancellationToken ct = default);
    Task<PagedList<Campaign>> ListAsync(
        Guid? organizationId,
        bool includeGlobal,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
