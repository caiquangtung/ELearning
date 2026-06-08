using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeReindexWorker(
    InMemoryAiKnowledgeReindexQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AiKnowledgeReindexWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var courseId in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var indexingService = scope.ServiceProvider.GetRequiredService<IAiKnowledgeIndexingService>();
                await indexingService.ReindexAsync(courseId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AI knowledge background reindex failed for course {CourseId}.", courseId);
            }
        }
    }
}
