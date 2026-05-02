using ELearning.Domain.Aggregates.PromotionAggregate;

namespace ELearning.Core.Abstractions;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<Campaign?> GetByIdWithRulesAndCouponsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Campaign>> ListAsync(Guid? organizationId, int take, CancellationToken ct = default);
}

