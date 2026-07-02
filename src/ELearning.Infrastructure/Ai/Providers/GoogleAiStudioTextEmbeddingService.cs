using System.Net;
using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class GoogleAiStudioTextEmbeddingService(
    HttpClient httpClient,
    IOptions<AiOptions> options)
{
    private const string ProviderName = "GoogleAiStudio";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiTextEmbedding> EmbedAsync(AiTextEmbeddingRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        var model = config.ResolveRagEmbeddingModel();
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ai:RagEmbeddingModel is required when Ai:RagEmbeddingProvider is GoogleAiStudio.");

        var apiKey = config.ResolveRagEmbeddingApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Ai:RagEmbeddingApiKey or Ai:ApiKey is required when Ai:RagEmbeddingProvider is GoogleAiStudio.");

        var dimensions = Math.Clamp(config.RagEmbeddingDimensions, 1, 4096);
        var requestBody = BuildRequestBody(request, model, dimensions);
        var attempts = Math.Clamp(config.RagEmbeddingMaxRetries, 0, 5) + 1;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(config.RagEmbeddingTimeoutSeconds, 5, 180)));

            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildEmbedContentUri(config, model, apiKey));
                httpRequest.Headers.Add("x-goog-api-key", apiKey);
                httpRequest.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                if (response.IsSuccessStatusCode)
                    return ParseResponse(body, model, dimensions);

                lastError = AiTextEmbeddingProviderException.FromStatusCode(ProviderName, response.StatusCode, body);
                if (!ShouldRetry(response.StatusCode) || attempt == attempts)
                    throw lastError;
            }
            catch (Exception ex) when (attempt < attempts && IsRetriable(ex))
            {
                lastError = ex;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw AiTextEmbeddingProviderException.FromException(ProviderName, ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), ct);
        }

        throw lastError switch
        {
            AiTextEmbeddingProviderException providerException => providerException,
            Exception ex => AiTextEmbeddingProviderException.FromException(ProviderName, ex),
            _ => new AiTextEmbeddingProviderException("Google AI Studio embedding provider request failed.", true)
        };
    }

    private static object BuildRequestBody(AiTextEmbeddingRequest request, string model, int dimensions)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = NormalizeModelName(model),
            ["content"] = new
            {
                parts = new[]
                {
                    new { text = request.Text ?? string.Empty }
                }
            },
            ["taskType"] = ToGoogleTaskType(request.Purpose),
            ["outputDimensionality"] = dimensions
        };

        if (request.Purpose == AiTextEmbeddingPurpose.RetrievalDocument &&
            !string.IsNullOrWhiteSpace(request.Title))
        {
            body["title"] = request.Title.Trim();
        }

        return body;
    }

    private static AiTextEmbedding ParseResponse(string body, string fallbackModel, int expectedDimensions)
    {
        using var document = JsonDocument.Parse(body);
        var values = document.RootElement.GetProperty("embedding").GetProperty("values");
        var vector = new float[values.GetArrayLength()];
        for (var i = 0; i < vector.Length; i++)
            vector[i] = values[i].GetSingle();

        if (vector.Length != expectedDimensions)
        {
            throw new InvalidOperationException(
                $"Google AI Studio embedding provider returned {vector.Length} dimensions; expected {expectedDimensions}.");
        }

        EmbeddingVectorUtils.Normalize(vector);
        return new AiTextEmbedding(vector, ProviderName, NormalizeModelName(fallbackModel), expectedDimensions);
    }

    private static Uri BuildEmbedContentUri(AiOptions config, string model, string apiKey)
    {
        var baseUrl = config.ResolveGoogleAiStudioRagEmbeddingBaseUrl().TrimEnd('/');
        var modelName = NormalizeModelName(model);
        return new Uri($"{baseUrl}/{modelName}:embedContent?key={Uri.EscapeDataString(apiKey)}", UriKind.Absolute);
    }

    private static string NormalizeModelName(string model)
    {
        var trimmed = model.Trim().Trim('/');
        return trimmed.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"models/{trimmed}";
    }

    private static string ToGoogleTaskType(AiTextEmbeddingPurpose purpose) =>
        purpose switch
        {
            AiTextEmbeddingPurpose.RetrievalDocument => "RETRIEVAL_DOCUMENT",
            AiTextEmbeddingPurpose.RetrievalQuery => "RETRIEVAL_QUERY",
            AiTextEmbeddingPurpose.StatusProbe => "RETRIEVAL_QUERY",
            _ => "RETRIEVAL_QUERY"
        };

    private static bool ShouldRetry(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private static bool IsRetriable(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException ||
        ex is AiTextEmbeddingProviderException { IsRetriable: true };
}
