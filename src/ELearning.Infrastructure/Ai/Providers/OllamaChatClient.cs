using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OllamaChatClient(
    HttpClient httpClient,
    IOptions<AiOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<OllamaChatResult> CompleteJsonAsync(
        string systemPrompt,
        string userPrompt,
        bool forceJson = true,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var model = string.IsNullOrWhiteSpace(config.OllamaModel) ? "qwen2.5:7b" : config.OllamaModel.Trim();

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false,
            format = forceJson ? "json" : null,
            options = new
            {
                temperature = 0.2m,
                num_predict = Math.Clamp(config.MaxOutputTokens, 256, 16_000)
            }
        };

        var attempts = Math.Clamp(config.MaxRetries, 0, 5) + 1;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var timeoutSec = config.OllamaTimeoutSeconds > 0 ? config.OllamaTimeoutSeconds : config.TimeoutSeconds;
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(timeoutSec, 5, 300)));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, BuildChatUri(config));
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.SendAsync(request, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                if (response.IsSuccessStatusCode)
                    return ParseResponse(body, model);

                lastError = new InvalidOperationException(
                    $"Ollama provider returned {(int)response.StatusCode}: {TrimForError(body)}");

                if (!ShouldRetry(response.StatusCode) || attempt == attempts)
                    throw lastError;
            }
            catch (Exception ex) when (attempt < attempts && IsRetriable(ex))
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), ct);
        }

        throw lastError ?? new InvalidOperationException("Ollama provider request failed.");
    }

    private static OllamaChatResult ParseResponse(string body, string fallbackModel)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("message", out var messageElement) ||
            !messageElement.TryGetProperty("content", out var contentElement))
        {
            throw new InvalidOperationException("Ollama response did not include message content.");
        }

        var content = contentElement.GetString();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Ollama provider returned an empty message.");

        var model = root.TryGetProperty("model", out var modelElement)
            ? modelElement.GetString()
            : fallbackModel;

        int? totalTokens = null;
        if (root.TryGetProperty("prompt_eval_count", out var promptEval) &&
            promptEval.TryGetInt32(out var promptCount) &&
            root.TryGetProperty("eval_count", out var eval) &&
            eval.TryGetInt32(out var evalCount))
        {
            totalTokens = promptCount + evalCount;
        }

        return new OllamaChatResult(
            "Ollama",
            string.IsNullOrWhiteSpace(model) ? fallbackModel : model,
            content,
            totalTokens);
    }

    private static Uri BuildChatUri(AiOptions config)
    {
        var baseUrl = string.IsNullOrWhiteSpace(config.OllamaBaseUrl)
            ? "http://localhost:11434"
            : config.OllamaBaseUrl.Trim();

        return new Uri($"{baseUrl.TrimEnd('/')}/api/chat", UriKind.Absolute);
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

public sealed record OllamaChatResult(
    string Provider,
    string Model,
    string Content,
    int? TokenEstimate);
