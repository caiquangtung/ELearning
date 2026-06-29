using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeReindexQueue(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IAiKnowledgeReindexQueue
{
    public async Task<Guid> EnqueueAsync(Guid? courseId, CancellationToken ct = default)
    {
        var job = AiKnowledgeReindexJob.Create(courseId, currentUserService.UserId);
        await context.AiKnowledgeReindexJobs.AddAsync(job, ct);
        await context.SaveChangesAsync(ct);
        return job.Id;
    }
}
