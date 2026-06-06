using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OpenAiCompatibleChatClient(
    HttpClient httpClient,
    IOptions<AiOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OpenAiCompatibleChatResult> CompleteJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var model = config.ResolveChatModel();
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ai:ChatModel is required when Ai:Provider is OpenAiCompatible.");

        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("Ai:ApiKey is required when Ai:Provider is OpenAiCompatible.");

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2m,
            max_tokens = Math.Clamp(config.MaxOutputTokens, 256, 16_000),
            response_format = new { type = "json_object" }
        };

        var attempts = Math.Clamp(config.MaxRetries, 0, 5) + 1;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(config.TimeoutSeconds, 5, 180)));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, BuildChatCompletionsUri(config));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey.Trim());
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.SendAsync(request, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                if (response.IsSuccessStatusCode)
                    return ParseResponse(body, model);

                lastError = new InvalidOperationException(
                    $"OpenAI-compatible provider returned {(int)response.StatusCode}: {TrimForError(body)}");

                if (!ShouldRetry(response.StatusCode) || attempt == attempts)
                    throw lastError;
            }
            catch (Exception ex) when (attempt < attempts && IsRetriable(ex))
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), ct);
        }

        throw lastError ?? new InvalidOperationException("OpenAI-compatible provider request failed.");
    }

    private static OpenAiCompatibleChatResult ParseResponse(string body, string fallbackModel)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        var content = root.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("OpenAI-compatible provider returned an empty message.");

        var model = root.TryGetProperty("model", out var modelElement)
            ? modelElement.GetString()
            : fallbackModel;

        int? totalTokens = null;
        if (root.TryGetProperty("usage", out var usage) &&
            usage.TryGetProperty("total_tokens", out var totalTokensElement) &&
            totalTokensElement.TryGetInt32(out var parsedTokens))
        {
            totalTokens = parsedTokens;
        }

        return new OpenAiCompatibleChatResult(
            "OpenAiCompatible",
            string.IsNullOrWhiteSpace(model) ? fallbackModel : model,
            content,
            totalTokens);
    }

    private static Uri BuildChatCompletionsUri(AiOptions config)
    {
        var baseUrl = string.IsNullOrWhiteSpace(config.BaseUrl)
            ? "https://api.openai.com/v1"
            : config.BaseUrl.Trim();

        if (baseUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            return new Uri(baseUrl, UriKind.Absolute);

        return new Uri($"{baseUrl.TrimEnd('/')}/chat/completions", UriKind.Absolute);
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

public sealed record OpenAiCompatibleChatResult(
    string Provider,
    string Model,
    string Content,
    int? TokenEstimate);
