using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;

namespace ELearning.Infrastructure.Ai;

public sealed class AiRequestLogRepository(ApplicationDbContext context)
    : GenericRepository<AiRequestLog>(context), IAiRequestLogRepository
{
}
