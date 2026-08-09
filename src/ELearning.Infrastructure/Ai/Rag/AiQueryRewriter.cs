using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Ai;

public sealed partial class AiQueryRewriter(ILogger<AiQueryRewriter> logger) : IAiQueryRewriter
{
    private static readonly Dictionary<string, string> AbbreviationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "DI", "Dependency Injection" },
        { "EF", "Entity Framework" },
        { "CQRS", "Command Query Responsibility Segregation" },
        { "JWT", "JSON Web Token" },
        { "DTO", "Data Transfer Object" },
        { "OR-M", "Object-Relational Mapping" },
        { "ORM", "Object-Relational Mapping" },
        { "API", "Application Programming Interface" },
        { "OOP", "Object-Oriented Programming" }
    };

    private static readonly string[] Pronouns =
    [
        "nó", "cái này", "phương thức này", "lớp này", "tính năng này", "bài này", "phần này",
        "khóa này", "this", "it", "that", "these", "those"
    ];

    public Task<string> RewriteQueryAsync(
        string question,
        IReadOnlyList<AiChatMessageContext> chatHistory,
        CancellationToken ct = default)
    {
        var normalized = question.Trim();
        if (normalized.Length == 0)
            return Task.FromResult(normalized);

        var rewritten = normalized;

        // 1. Context-aware pronoun resolution from chat history
        if (chatHistory.Count > 0 && ContainsPronoun(rewritten))
        {
            var recentSubject = ExtractSubjectFromHistory(chatHistory);
            if (!string.IsNullOrWhiteSpace(recentSubject))
            {
                rewritten = ResolvePronoun(rewritten, recentSubject);
            }
        }

        // 2. Expand common technical abbreviations for hybrid search recall
        rewritten = ExpandAbbreviations(rewritten);

        if (!string.Equals(normalized, rewritten, StringComparison.Ordinal))
        {
            logger.LogInformation(
                "Query rewriter transformed query. Original='{Original}', Rewritten='{Rewritten}'",
                normalized,
                rewritten);
        }

        return Task.FromResult(rewritten);
    }

    private static bool ContainsPronoun(string text)
    {
        var lower = text.ToLowerInvariant();
        return Pronouns.Any(p => lower.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractSubjectFromHistory(IReadOnlyList<AiChatMessageContext> chatHistory)
    {
        // Search backwards through history for substantive terms/subjects in user or assistant turns
        foreach (var msg in chatHistory.Reverse())
        {
            if (string.IsNullOrWhiteSpace(msg.Content))
                continue;

            var words = msg.Content.Split(
                new[] { ' ', ',', '.', '!', '?', ';', ':', '\t', '\n', '\r', '"', '\'', '(', ')' },
                StringSplitOptions.RemoveEmptyEntries);

            var candidate = words.FirstOrDefault(w =>
                w.Length >= 3 &&
                !Pronouns.Contains(w, StringComparer.OrdinalIgnoreCase) &&
                char.IsUpper(w[0]));

            if (!string.IsNullOrWhiteSpace(candidate))
                return candidate.Trim();
        }

        // Fallback: pick the first substantive noun phrase from the last user message
        var lastUserMsg = chatHistory.LastOrDefault(m => string.Equals(m.Role, "User", StringComparison.OrdinalIgnoreCase));
        if (lastUserMsg is not null)
        {
            var terms = lastUserMsg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 4 && !Pronouns.Contains(t, StringComparer.OrdinalIgnoreCase))
                .Take(2);
            var joined = string.Join(" ", terms);
            if (!string.IsNullOrWhiteSpace(joined))
                return joined.Trim();
        }

        return string.Empty;
    }

    private static string ResolvePronoun(string text, string subject)
    {
        var result = text;
        foreach (var p in Pronouns)
        {
            var pattern = $@"\b{Regex.Escape(p)}\b";
            result = Regex.Replace(result, pattern, subject, RegexOptions.IgnoreCase);
        }
        return result;
    }

    private static string ExpandAbbreviations(string text)
    {
        var result = text;
        foreach (var (abbr, expansion) in AbbreviationMap)
        {
            // Only expand if abbreviation appears as a whole word and the expanded form is not already present
            var pattern = $@"\b{Regex.Escape(abbr)}\b";
            if (Regex.IsMatch(result, pattern, RegexOptions.IgnoreCase) &&
                !result.Contains(expansion, StringComparison.OrdinalIgnoreCase))
            {
                result = Regex.Replace(result, pattern, $"{abbr} ({expansion})", RegexOptions.IgnoreCase);
            }
        }
        return result;
    }
}
