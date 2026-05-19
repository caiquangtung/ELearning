using ELearning.Core.Common;
using ELearning.Domain.Aggregates.NotificationAggregate;

namespace ELearning.Core.Abstractions;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PagedList<Notification>> ListForUserAsync(Guid userId, int page, int pageSize, bool unreadOnly, CancellationToken ct = default);
    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);
}
