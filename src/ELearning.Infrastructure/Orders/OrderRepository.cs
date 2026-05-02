using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Orders;

public sealed class OrderRepository(ApplicationDbContext context)
    : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> ListForBuyerAsync(Guid buyerUserId, int take, CancellationToken ct = default)
    {
        take = take is <= 0 or > 200 ? 50 : take;
        return await DbSet.AsNoTracking()
            .Where(o => o.BuyerUserId == buyerUserId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}

