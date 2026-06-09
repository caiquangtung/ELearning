using System.Threading.Channels;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;

namespace ELearning.Infrastructure.Ai;

public sealed class InMemoryAiKnowledgeReindexChannel
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask WriteAsync(Guid jobId, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(jobId, ct);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);
}

public sealed class AiKnowledgeReindexQueue(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    InMemoryAiKnowledgeReindexChannel channel)
    : IAiKnowledgeReindexQueue
{
    public async Task<Guid> EnqueueAsync(Guid? courseId, CancellationToken ct = default)
    {
        var job = AiKnowledgeReindexJob.Create(courseId, currentUserService.UserId);
        await context.AiKnowledgeReindexJobs.AddAsync(job, ct);
        await context.SaveChangesAsync(ct);
        await channel.WriteAsync(job.Id, ct);
        return job.Id;
    }
}
