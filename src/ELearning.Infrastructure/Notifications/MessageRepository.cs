using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;

namespace ELearning.Infrastructure.Notifications;

public sealed class MessageRepository(ApplicationDbContext context)
    : GenericRepository<Message>(context), IMessageRepository
{
}
