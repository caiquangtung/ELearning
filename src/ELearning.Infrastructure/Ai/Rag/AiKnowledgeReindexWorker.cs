using ELearning.Application.Common.Interfaces;
using ELearning.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeReindexWorker(
    InMemoryAiKnowledgeReindexChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<AiKnowledgeReindexWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobId in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var job = await context.AiKnowledgeReindexJobs.FindAsync([jobId], stoppingToken);
                if (job is null)
                    continue;

                var indexingService = scope.ServiceProvider.GetRequiredService<IAiKnowledgeIndexingService>();
                await indexingService.ReindexAsync(
                    job.CourseId,
                    job.RequestedByUserId,
                    job.Id,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AI knowledge background reindex failed for job {JobId}.", jobId);
            }
        }
    }
}
