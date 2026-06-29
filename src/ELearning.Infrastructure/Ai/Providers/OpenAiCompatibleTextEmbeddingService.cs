using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OpenAiCompatibleTextEmbeddingService(
    HttpClient httpClient,
    IOptions<AiOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiTextEmbedding> EmbedAsync(string text, CancellationToken ct = default)
    {
        var config = options.Value;
        var model = config.RagEmbeddingModel.Trim();
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ai:RagEmbeddingModel is required when Ai:RagEmbeddingProvider is OpenAiCompatible.");

        var apiKey = config.ResolveRagEmbeddingApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Ai:RagEmbeddingApiKey or Ai:ApiKey is required when Ai:RagEmbeddingProvider is OpenAiCompatible.");

        var dimensions = Math.Clamp(config.RagEmbeddingDimensions, 1, 4096);
        var requestBody = new
        {
            model,
            input = text ?? string.Empty,
            dimensions
        };

        var attempts = Math.Clamp(config.RagEmbeddingMaxRetries, 0, 5) + 1;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(config.RagEmbeddingTimeoutSeconds, 5, 180)));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, BuildEmbeddingsUri(config));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.SendAsync(request, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                if (response.IsSuccessStatusCode)
                    return ParseResponse(body, model, dimensions);

                lastError = new InvalidOperationException(
                    $"OpenAI-compatible embedding provider returned {(int)response.StatusCode}: {TrimForError(body)}");

                if (!ShouldRetry(response.StatusCode) || attempt == attempts)
                    throw lastError;
            }
            catch (Exception ex) when (attempt < attempts && IsRetriable(ex))
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), ct);
        }

        throw lastError ?? new InvalidOperationException("OpenAI-compatible embedding provider request failed.");
    }

    private static AiTextEmbedding ParseResponse(string body, string fallbackModel, int expectedDimensions)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var embeddingElement = root.GetProperty("data")[0].GetProperty("embedding");
        var vector = new float[embeddingElement.GetArrayLength()];
        for (var i = 0; i < vector.Length; i++)
            vector[i] = embeddingElement[i].GetSingle();

        if (vector.Length != expectedDimensions)
        {
            throw new InvalidOperationException(
                $"OpenAI-compatible embedding provider returned {vector.Length} dimensions; expected {expectedDimensions}.");
        }

        EmbeddingVectorUtils.Normalize(vector);

        var model = root.TryGetProperty("model", out var modelElement)
            ? modelElement.GetString()
            : fallbackModel;

        return new AiTextEmbedding(
            vector,
            "OpenAiCompatible",
            string.IsNullOrWhiteSpace(model) ? fallbackModel : model,
            expectedDimensions);
    }

    private static Uri BuildEmbeddingsUri(AiOptions config)
    {
        var baseUrl = config.ResolveRagEmbeddingBaseUrl();
        if (baseUrl.EndsWith("/embeddings", StringComparison.OrdinalIgnoreCase))
            return new Uri(baseUrl, UriKind.Absolute);

        return new Uri($"{baseUrl.TrimEnd('/')}/embeddings", UriKind.Absolute);
    }

    private static bool ShouldRetry(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private static bool IsRetriable(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException;

    private static string TrimForError(string value)
    {
        var normalized = value.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 300 ? normalized : normalized[..300];
    }
}
