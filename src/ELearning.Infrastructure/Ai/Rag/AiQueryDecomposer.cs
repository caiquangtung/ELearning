using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Ai;

public sealed partial class AiQueryDecomposer : IAiQueryDecomposer
{
    private const int MaxSubQueries = 3;

    private static readonly string[] ComparisonTriggers =
    [
        "so sánh", "khác nhau", "khác biệt", "compare", "versus", "vs.", "vs"
    ];

    private static readonly string[] ConjunctionTriggers =
    [
        " và ", " cùng với ", " bên cạnh ", " đồng thời ", " and ", " plus "
    ];

    public AiQueryDecompositionResult DecomposeQuery(string question)
    {
        var normalized = question.Trim();
        if (normalized.Length == 0)
        {
            return new AiQueryDecompositionResult(normalized, [], false, "Empty question");
        }

        var lower = normalized.ToLowerInvariant();

        // 1. Check for comparison query patterns
        if (ComparisonTriggers.Any(t => lower.Contains(t, StringComparison.OrdinalIgnoreCase)))
        {
            var comparisonResult = TryDecomposeComparison(normalized);
            if (comparisonResult is not null && comparisonResult.SubQueries.Count > 1)
            {
                return comparisonResult;
            }
        }

        // 2. Check for multi-topic conjunction patterns
        if (ConjunctionTriggers.Any(t => lower.Contains(t, StringComparison.OrdinalIgnoreCase)))
        {
            var conjunctionResult = TryDecomposeConjunction(normalized);
            if (conjunctionResult is not null && conjunctionResult.SubQueries.Count > 1)
            {
                return conjunctionResult;
            }
        }

        // Default: Single topic query
        return new AiQueryDecompositionResult(
            normalized,
            [normalized],
            false,
            "Single topic query - no decomposition required");
    }

    private static AiQueryDecompositionResult? TryDecomposeComparison(string question)
    {
        // Example: "So sánh Dependency Injection và Service Locator trong C#"
        // Pattern: (So sánh|Compare) {TopicA} (và|vs|versus) {TopicB} [(trong|in) {Context}]
        var match = ComparisonRegex().Match(question);
        if (match.Success)
        {
            var topicA = match.Groups["topicA"].Value.Trim();
            var topicB = match.Groups["topicB"].Value.Trim();
            var context = match.Groups["context"].Success ? match.Groups["context"].Value.Trim() : string.Empty;

            var contextSuffix = string.IsNullOrWhiteSpace(context) ? string.Empty : $" {context}";

            var subQueries = new List<string>
            {
                $"{topicA}{contextSuffix}",
                $"{topicB}{contextSuffix}",
                question
            };

            return new AiQueryDecompositionResult(
                question,
                subQueries.Take(MaxSubQueries).ToList(),
                true,
                "Decomposed multi-topic comparison query into focused target subjects");
        }

        return null;
    }

    private static AiQueryDecompositionResult? TryDecomposeConjunction(string question)
    {
        // Example: "Giải thích JWT Authentication và cách cấu hình Middleware"
        var split = question.Split(
            new[] { " và ", " cùng với ", " đồng thời ", " and ", " plus " },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (split.Length >= 2)
        {
            var subQueries = split
                .Where(s => s.Length >= 5)
                .Take(MaxSubQueries)
                .ToList();

            if (subQueries.Count >= 2)
            {
                return new AiQueryDecompositionResult(
                    question,
                    subQueries,
                    true,
                    "Decomposed multi-topic conjunctive query into independent sub-queries");
            }
        }

        return null;
    }

    [GeneratedRegex(@"(?:so sánh|khác nhau|compare|versus|vs\.?)\s+(?<topicA>.+?)\s+(?:và|vs\.?|versus|and)\s+(?<topicB>.+?)(?:\s+(?<context>(?:trong|in)\s+.+))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ComparisonRegex();
}
