using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed partial class LocalDenseTextEmbeddingService : IAiTextEmbeddingService
{
    public const int DefaultEmbeddingDimensions = 768;
    private const string ProviderName = "Local";
    private const string ModelNamePrefix = "local-dense-hash";
    private readonly IOptions<AiOptions>? options;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "into", "this", "that", "your", "you", "are",
        "course", "lesson", "learn", "learning", "fundamentals", "introduction", "basic",
        "advanced", "want", "become", "build", "create", "using", "about", "need"
    };

    public LocalDenseTextEmbeddingService(IOptions<AiOptions>? options = null)
    {
        this.options = options;
    }

    public Task<AiTextEmbedding> EmbedAsync(AiTextEmbeddingRequest request, CancellationToken ct = default)
        => EmbedAsync(request.Text, ct);

    public Task<AiTextEmbedding> EmbedAsync(string text, CancellationToken ct = default)
    {
        var dimensions = Math.Clamp(options?.Value.RagEmbeddingDimensions ?? DefaultEmbeddingDimensions, 1, 4096);
        var vector = new float[dimensions];
        var tokens = Tokenize(text).ToList();
        if (tokens.Count == 0 && !string.IsNullOrWhiteSpace(text))
            tokens.Add("fallback:" + text.Trim().ToLowerInvariant());

        foreach (var token in tokens)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var index = (int)(BitConverter.ToUInt32(hash, 0) % (uint)dimensions);
            var sign = (hash[4] & 1) == 0 ? 1f : -1f;
            vector[index] += sign;
        }

        EmbeddingVectorUtils.Normalize(vector);
        return Task.FromResult(new AiTextEmbedding(vector, ProviderName, $"{ModelNamePrefix}-{dimensions}-v1", dimensions));
    }

    private static IEnumerable<string> Tokenize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            yield break;

        foreach (Match match in WordRegex().Matches(value.ToLowerInvariant()))
        {
            var token = match.Value;
            if (token.Length < 3 || StopWords.Contains(token))
                continue;

            yield return NormalizeToken(token);
        }
    }

    private static string NormalizeToken(string token)
    {
        if (token.EndsWith("ing", StringComparison.OrdinalIgnoreCase) && token.Length > 5)
            return token[..^3];
        if (token.EndsWith("ed", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            return token[..^2];
        if (token.EndsWith("s", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            return token[..^1];
        return token;
    }

    [GeneratedRegex("[\\p{L}\\p{N}]+", RegexOptions.Compiled)]
    private static partial Regex WordRegex();
}
