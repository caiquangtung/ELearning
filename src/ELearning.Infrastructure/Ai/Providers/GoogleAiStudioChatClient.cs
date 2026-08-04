using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class GoogleAiStudioChatClient(
    HttpClient httpClient,
    IOptions<AiOptions> options)
{
    private const string ProviderName = "GoogleAiStudio";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GoogleAiStudioChatResult> CompleteJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var apiKey = config.ResolveRagEmbeddingApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = config.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Ai:ApiKey or Ai:RagEmbeddingApiKey is required when using Google AI Studio chat.");

        var model = config.ResolveChatModel();
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ai:ChatModel is required when Ai:Provider is GoogleAiStudio.");

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userPrompt } }
                }
            },
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            generationConfig = new
            {
                temperature = 0.2m,
                maxOutputTokens = Math.Clamp(config.MaxOutputTokens, 256, 16_000)
            }
        };

        var attempts = Math.Clamp(config.MaxRetries, 0, 5) + 1;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(config.TimeoutSeconds, 5, 180)));

            try
            {
                var uri = BuildGenerateContentUri(config, model, apiKey);
                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.SendAsync(request, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                if (response.IsSuccessStatusCode)
                    return ParseResponse(body, model);

                lastError = new InvalidOperationException(
                    $"Google AI Studio returned {(int)response.StatusCode}: {TrimForError(body)}");

                if (!ShouldRetry(response.StatusCode) || attempt == attempts)
                    throw lastError;
            }
            catch (Exception ex) when (attempt < attempts && IsRetriable(ex))
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), ct);
        }

        throw lastError ?? new InvalidOperationException("Google AI Studio request failed.");
    }

    private static Uri BuildGenerateContentUri(AiOptions config, string model, string apiKey)
    {
        var baseUrl = config.ResolveGoogleAiStudioRagEmbeddingBaseUrl().TrimEnd('/');
        var modelName = NormalizeModelName(model);
        return new Uri($"{baseUrl}/{modelName}:generateContent?key={Uri.EscapeDataString(apiKey)}", UriKind.Absolute);
    }

    private static string NormalizeModelName(string model)
    {
        var trimmed = model.Trim().Trim('/');
        return trimmed.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"models/{trimmed}";
    }

    private static GoogleAiStudioChatResult ParseResponse(string body, string fallbackModel)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Google AI Studio response did not include any candidates.");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Google AI Studio response did not include content parts.");
        }

        var text = ExtractText(parts);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Google AI Studio returned an empty response.");

        var model = root.TryGetProperty("modelVersion", out var modelElement)
            ? modelElement.GetString()
            : fallbackModel;

        int? totalTokens = null;
        if (root.TryGetProperty("usageMetadata", out var usage) &&
            usage.TryGetProperty("totalTokenCount", out var totalTokensElement) &&
            totalTokensElement.TryGetInt32(out var parsedTokens))
        {
            totalTokens = parsedTokens;
        }

        return new GoogleAiStudioChatResult(
            ProviderName,
            string.IsNullOrWhiteSpace(model) ? fallbackModel : model,
            text,
            totalTokens);
    }

    private static string ExtractText(JsonElement parts)
    {
        var builder = new StringBuilder();

        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("text", out var textElement))
                continue;

            var text = textElement.GetString();
            if (!string.IsNullOrWhiteSpace(text))
                builder.Append(text);
        }

        return builder.ToString().Trim();
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

public sealed record GoogleAiStudioChatResult(
    string Provider,
    string Model,
    string Content,
    int? TokenEstimate);
