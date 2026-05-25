using ELearning.Core.Abstractions;
using ELearning.Core.Common;
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

    public async Task<PagedList<Order>> ListForBuyerAsync(
        Guid buyerUserId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var query = DbSet
            .AsNoTracking()
            .Where(o => o.BuyerUserId == buyerUserId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Order>.Create(items, page, pageSize, total);
    }
}
