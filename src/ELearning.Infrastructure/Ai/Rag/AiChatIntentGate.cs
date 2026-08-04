using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class AiChatIntentGate(IOptions<AiOptions> options)
{
    private static readonly HashSet<string> GreetingTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello",
        "hi",
        "hey",
        "good morning",
        "good afternoon",
        "good evening",
        "how are you",
        "how's it going",
        "what's up",
        "greetings",
        "good day",
        "hi there",
        "hello there",
        "hey there",
        "yo",
        "sup",
        "howdy",
        "morning",
        "afternoon",
        "evening",
        "what is your name",
        "who are you",
        "tell me about yourself",
        "introduce yourself"
    };

    private static readonly StringComparer TokenComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly HashSet<string> SkipTokens = new(TokenComparer)
    {
        "a", "an", "the", "is", "are", "was", "were", "be", "been", "being",
        "have", "has", "had", "do", "does", "did", "will", "would", "could",
        "should", "may", "might", "shall", "can", "need", "dare", "ought",
        "to", "of", "in", "for", "on", "with", "at", "by", "from", "as",
        "into", "through", "during", "before", "after", "above", "below",
        "between", "out", "off", "over", "under", "again", "further", "then",
        "once", "here", "there", "when", "where", "why", "how", "all", "each",
        "every", "both", "few", "more", "most", "other", "some", "such", "no",
        "not", "only", "own", "same", "so", "than", "too", "very", "just",
        "because", "but", "and", "or", "if", "while", "about", "up", "what",
        "which", "who", "whom", "this", "that", "these", "those", "i", "me",
        "my", "we", "our", "you", "your", "he", "him", "his", "she", "her",
        "it", "its", "they", "them", "their", "much", "many", "lot", "lots",
        "please", "thanks", "thank", "sorry", "ok", "okay", "yes", "no", "yeah",
        "nice", "great", "good", "bad", "cool", "awesome", "fine"
    };

    public AiChatIntentResult Evaluate(string question)
    {
        if (!options.Value.RagEnableIntentGating)
            return AiChatIntentResult.Relevant();

        var normalized = question.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return AiChatIntentResult.Irrelevant("empty");

        if (IsGreeting(normalized))
            return AiChatIntentResult.Greeting();

        if (!HasSubstantiveContent(normalized))
            return AiChatIntentResult.Irrelevant("generic");

        return AiChatIntentResult.Relevant();
    }

    private bool IsGreeting(string question)
    {
        var lower = question.ToLowerInvariant();
        if (GreetingTokens.Contains(lower))
            return true;

        foreach (var token in GreetingTokens)
        {
            if (lower.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                var nextCharIndex = token.Length;
                if (nextCharIndex >= lower.Length || char.IsWhiteSpace(lower[nextCharIndex]) || lower[nextCharIndex] is ',' or '!' or '?')
                    return true;
            }
        }

        var words = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 3 && words.Any(w => GreetingTokens.Contains(w)))
            return true;

        return false;
    }

    private bool HasSubstantiveContent(string question)
    {
        var words = question.Split(
            new[] { ' ', ',', '.', '!', '?', ';', ':', '\t', '\n', '\r', '"', '\'' },
            StringSplitOptions.RemoveEmptyEntries);

        var substantive = words
            .Select(w => w.Trim().Trim('-', '_'))
            .Where(w => w.Length >= 3)
            .Where(w => !SkipTokens.Contains(w))
            .ToList();

        return substantive.Count >= 2;
    }
}

public sealed record AiChatIntentResult(bool SkipRetrieval, bool IsGreeting, string? Reason)
{
    public static AiChatIntentResult Greeting() => new(true, true, "greeting");
    public static AiChatIntentResult Irrelevant(string reason) => new(true, false, reason);
    public static AiChatIntentResult Relevant() => new(false, false, null);
}
