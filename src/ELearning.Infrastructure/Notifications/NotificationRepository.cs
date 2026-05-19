using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Notifications;

public sealed class NotificationRepository(ApplicationDbContext context)
    : GenericRepository<Notification>(context), INotificationRepository
{
    public async Task<Notification?> GetForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

    public async Task<PagedList<Notification>> ListForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        bool unreadOnly,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => n.ReadAt == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Notification>.Create(items, page, pageSize, total);
    }

    public async Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.CountAsync(n => n.UserId == userId && n.ReadAt == null, ct);
}
