using ELearning.Application.Common.Interfaces;
using ELearning.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ELearning.Infrastructure.Ai;

public sealed class AiKnowledgeReindexWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AiOptions> options,
    ILogger<AiKnowledgeReindexWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ResetStaleInProgressJobsAsync(context, stoppingToken);

                var job = await TryClaimNextJobAsync(context, stoppingToken);
                if (job is null)
                {
                    await Task.Delay(PollDelay(), stoppingToken);
                    continue;
                }

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
                logger.LogWarning(ex, "AI knowledge background reindex worker failed.");
                await Task.Delay(PollDelay(), stoppingToken);
            }
        }
    }

    private TimeSpan PollDelay() =>
        TimeSpan.FromSeconds(Math.Clamp(options.Value.RagReindexPollSeconds, 1, 60));

    private static async Task<ClaimedReindexJob?> TryClaimNextJobAsync(
        ApplicationDbContext context,
        CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                UPDATE ai_knowledge_reindex_jobs
                SET status = 'InProgress',
                    started_at = COALESCE(started_at, @now),
                    completed_at = NULL,
                    error = NULL,
                    updated_at = @now
                WHERE id = (
                    SELECT id
                    FROM ai_knowledge_reindex_jobs
                    WHERE status = 'Queued'
                    ORDER BY created_at
                    FOR UPDATE SKIP LOCKED
                    LIMIT 1
                )
                RETURNING id, course_id, requested_by_user_id;
                """;
            command.Parameters.Add(new NpgsqlParameter("now", DateTime.UtcNow));

            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            return new ClaimedReindexJob(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetGuid(1),
                reader.IsDBNull(2) ? null : reader.GetGuid(2));
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static async Task ResetStaleInProgressJobsAsync(
        ApplicationDbContext context,
        CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                UPDATE ai_knowledge_reindex_jobs
                SET status = 'Queued',
                    error = 'Requeued after stale in-progress state.',
                    updated_at = @now
                WHERE status = 'InProgress'
                    AND started_at IS NOT NULL
                    AND started_at < @cutoff;
                """;
            var now = DateTime.UtcNow;
            command.Parameters.Add(new NpgsqlParameter("now", now));
            command.Parameters.Add(new NpgsqlParameter("cutoff", now.AddMinutes(-30)));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private sealed record ClaimedReindexJob(Guid Id, Guid? CourseId, Guid? RequestedByUserId);
}
