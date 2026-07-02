using System.Net;

namespace ELearning.Infrastructure.Ai;

public sealed class AiTextEmbeddingProviderException : Exception
{
    public AiTextEmbeddingProviderException(string message, bool isRetriable, Exception? innerException = null)
        : base(message, innerException)
    {
        IsRetriable = isRetriable;
    }

    public bool IsRetriable { get; }

    public static AiTextEmbeddingProviderException FromStatusCode(
        string provider,
        HttpStatusCode statusCode,
        string body)
    {
        var retriable = statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
        return new AiTextEmbeddingProviderException(
            $"{provider} embedding provider returned {(int)statusCode}: {TrimForError(body)}",
            retriable);
    }

    public static AiTextEmbeddingProviderException FromException(string provider, Exception exception) =>
        new(
            $"{provider} embedding provider request failed: {exception.Message}",
            exception is HttpRequestException or TaskCanceledException,
            exception);

    private static string TrimForError(string value)
    {
        var normalized = value.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 300 ? normalized : normalized[..300];
    }
}
