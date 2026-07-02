using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeReindexQueue(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IOptions<AiOptions> options)
    : IAiKnowledgeReindexQueue
{
    public async Task<Guid> EnqueueAsync(Guid? courseId, CancellationToken ct = default)
    {
        if (!options.Value.RagAutoReindexEnabled)
            return Guid.Empty;

        var job = AiKnowledgeReindexJob.Create(courseId, currentUserService.UserId);
        await context.AiKnowledgeReindexJobs.AddAsync(job, ct);
        await context.SaveChangesAsync(ct);
        return job.Id;
    }
}
