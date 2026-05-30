using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Ai;

public sealed partial class LocalEmbeddingService : IAiEmbeddingService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "into", "this", "that", "your", "you", "are",
        "course", "lesson", "learn", "learning", "fundamentals", "introduction", "basic",
        "advanced", "want", "become", "build", "create", "using", "about", "need"
    };

    public IReadOnlyDictionary<string, decimal> Embed(string text)
    {
        var tokens = Tokenize(text).ToList();
        if (tokens.Count == 0)
            return new Dictionary<string, decimal>();

        return tokens
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => Math.Round((decimal)g.Count() / tokens.Count, 6),
                StringComparer.OrdinalIgnoreCase);
    }

    public decimal CosineSimilarity(IReadOnlyDictionary<string, decimal> left, IReadOnlyDictionary<string, decimal> right)
    {
        if (left.Count == 0 || right.Count == 0)
            return 0m;

        var dot = left.Sum(pair => pair.Value * right.GetValueOrDefault(pair.Key));
        var leftNorm = Math.Sqrt((double)left.Values.Sum(x => x * x));
        var rightNorm = Math.Sqrt((double)right.Values.Sum(x => x * x));
        if (leftNorm == 0 || rightNorm == 0)
            return 0m;

        return Math.Round((decimal)((double)dot / (leftNorm * rightNorm)), 6);
    }

    public IReadOnlyList<string> TopSharedTerms(
        IReadOnlyDictionary<string, decimal> left,
        IReadOnlyDictionary<string, decimal> right,
        int limit)
    {
        return left.Keys
            .Where(right.ContainsKey)
            .OrderByDescending(term => left[term] + right[term])
            .ThenBy(term => term)
            .Take(Math.Max(1, limit))
            .ToList();
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

            yield return Normalize(token);
        }
    }

    private static string Normalize(string token)
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
