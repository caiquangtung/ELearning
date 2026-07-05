using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ELearning.Infrastructure.Ai;

public sealed class ConfigurableAiTextEmbeddingService(
    LocalDenseTextEmbeddingService local,
    OpenAiCompatibleTextEmbeddingService openAiCompatible,
    GoogleAiStudioTextEmbeddingService googleAiStudio,
    ApplicationDbContext context,
    IOptions<AiOptions> options,
    ILogger<ConfigurableAiTextEmbeddingService> logger)
    : IAiTextEmbeddingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ConfigurableAiTextEmbeddingService(
        LocalDenseTextEmbeddingService local,
        OpenAiCompatibleTextEmbeddingService openAiCompatible,
        IOptions<AiOptions> options,
        ILogger<ConfigurableAiTextEmbeddingService> logger)
        : this(
            local,
            openAiCompatible,
            new GoogleAiStudioTextEmbeddingService(new HttpClient(), options),
            null!,
            options,
            logger)
    {
    }

    public Task<AiTextEmbedding> EmbedAsync(string text, CancellationToken ct = default) =>
        EmbedAsync(new AiTextEmbeddingRequest(text, AiTextEmbeddingPurpose.StatusProbe), ct);

    public async Task<AiTextEmbedding> EmbedAsync(AiTextEmbeddingRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.UsesRemoteRagEmbeddingProvider())
            return await local.EmbedAsync(request, ct);

        if (request.Purpose == AiTextEmbeddingPurpose.RetrievalQuery)
        {
            var cached = await TryGetCachedQueryEmbeddingAsync(config, request.Text, ct);
            if (cached is not null)
                return cached;
        }

        try
        {
            var embedding = config.UsesGoogleAiStudioRagEmbeddingProvider()
                ? await googleAiStudio.EmbedAsync(request, ct)
                : await openAiCompatible.EmbedAsync(request, ct);

            if (request.Purpose == AiTextEmbeddingPurpose.RetrievalQuery)
                await CacheQueryEmbeddingAsync(config, request.Text, embedding, ct);

            return embedding;
        }
        catch (Exception ex) when (config.UsesOpenAiCompatibleRagEmbeddingProvider() && config.FallbackToLocal)
        {
            logger.LogWarning(ex, "OpenAI-compatible RAG embedding provider failed; falling back to local dense embedding.");
            return await local.EmbedAsync(request, ct);
        }
    }

    private async Task<AiTextEmbedding?> TryGetCachedQueryEmbeddingAsync(
        AiOptions config,
        string query,
        CancellationToken ct)
    {
        var normalized = NormalizeQuery(query);
        if (normalized.Length == 0)
            return null;

        var provider = ResolveProvider(config);
        var model = ResolveModel(config);
        var dimensions = Math.Clamp(config.RagEmbeddingDimensions, 1, 4096);
        var hash = ComputeCacheHash(normalized);
        var now = DateTime.UtcNow;

        var cache = await context.AiQueryEmbeddingCache
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.QueryHash == hash &&
                    x.Provider == provider &&
                    x.Model == model &&
                    x.Dimensions == dimensions &&
                    x.ExpiresAt > now,
                ct);
        if (cache is null)
            return null;

        return new AiTextEmbedding(
            JsonSerializer.Deserialize<float[]>(cache.EmbeddingJson, JsonOptions) ?? [],
            cache.Provider,
            cache.Model,
            cache.Dimensions);
    }

    private async Task CacheQueryEmbeddingAsync(
        AiOptions config,
        string query,
        AiTextEmbedding embedding,
        CancellationToken ct)
    {
        var normalized = NormalizeQuery(query);
        if (normalized.Length == 0)
            return;

        var hash = ComputeCacheHash(normalized);
        var ttlDays = Math.Clamp(config.RagQueryEmbeddingCacheTtlDays, 1, 365);
        var expiresAt = DateTime.UtcNow.AddDays(ttlDays);
        var embeddingJson = JsonSerializer.Serialize(embedding.Vector, JsonOptions);

        var existing = await context.AiQueryEmbeddingCache.FirstOrDefaultAsync(
            x => x.QueryHash == hash &&
                x.Provider == embedding.Provider &&
                x.Model == embedding.Model &&
                x.Dimensions == embedding.Dimensions,
            ct);

        Guid cacheId;
        if (existing is null)
        {
            var cache = AiQueryEmbeddingCache.Create(
                hash,
                normalized,
                embedding.Provider,
                embedding.Model,
                embedding.Dimensions,
                embeddingJson,
                expiresAt);
            cacheId = cache.Id;
            await context.AiQueryEmbeddingCache.AddAsync(cache, ct);
        }
        else
        {
            cacheId = existing.Id;
            existing.Refresh(embeddingJson, expiresAt);
        }

        await context.SaveChangesAsync(ct);
        await UpdateCachedVectorAsync(cacheId, embedding, ct);
    }

    private async Task UpdateCachedVectorAsync(
        Guid cacheId,
        AiTextEmbedding embedding,
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
                UPDATE ai_query_embedding_cache
                SET embedding_vector = CAST(@embedding_vector AS vector(768))
                WHERE id = @id
                """;
            command.Parameters.Add(new NpgsqlParameter("embedding_vector", PgVectorFormatter.ToVectorLiteral(embedding.Vector)));
            command.Parameters.Add(new NpgsqlParameter("id", cacheId));
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static string ResolveProvider(AiOptions config) =>
        config.UsesGoogleAiStudioRagEmbeddingProvider() ? "GoogleAiStudio" : "OpenAiCompatible";

    private static string ResolveModel(AiOptions config) =>
        config.UsesGoogleAiStudioRagEmbeddingProvider()
            ? $"models/{config.ResolveRagEmbeddingModel().Trim().Trim('/').Replace("models/", "", StringComparison.OrdinalIgnoreCase)}"
            : config.ResolveRagEmbeddingModel();

    private static string NormalizeQuery(string? query) =>
        string.Join(' ', (query ?? string.Empty).Trim().ToLowerInvariant().Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string ComputeCacheHash(string normalizedQuery)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedQuery));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
