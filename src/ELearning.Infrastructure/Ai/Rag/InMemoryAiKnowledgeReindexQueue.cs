using System.Threading.Channels;
using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Ai;

public sealed class InMemoryAiKnowledgeReindexQueue : IAiKnowledgeReindexQueue
{
    private readonly Channel<Guid?> _channel = Channel.CreateUnbounded<Guid?>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(Guid? courseId, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(courseId, ct);

    public IAsyncEnumerable<Guid?> ReadAllAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);
}
