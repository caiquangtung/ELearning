using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Ai;

public sealed partial class LocalDenseTextEmbeddingService : IAiTextEmbeddingService
{
    public const int EmbeddingDimensions = 384;
    private const string ProviderName = "Local";
    private const string ModelName = "local-dense-hash-384-v1";

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "into", "this", "that", "your", "you", "are",
        "course", "lesson", "learn", "learning", "fundamentals", "introduction", "basic",
        "advanced", "want", "become", "build", "create", "using", "about", "need"
    };

    public AiTextEmbedding Embed(string text)
    {
        var dimensions = EmbeddingDimensions;
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

        Normalize(vector);
        return new AiTextEmbedding(vector, ProviderName, ModelName, dimensions);
    }

    private static void Normalize(float[] vector)
    {
        var sum = 0d;
        foreach (var value in vector)
            sum += value * value;

        var norm = Math.Sqrt(sum);
        if (norm <= 0)
            return;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);
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
