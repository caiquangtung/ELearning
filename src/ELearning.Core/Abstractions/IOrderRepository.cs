using ELearning.Domain.Aggregates.OrderAggregate;

namespace ELearning.Core.Abstractions;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListForBuyerAsync(Guid buyerUserId, int take, CancellationToken ct = default);
}

