using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class LocalEssayGradingService(IOptions<AiOptions> options) : IAiEssayGradingService
{
    public Task<AiEssayGradingResult> SuggestAsync(AiEssayGradingRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        var suggestions = request.Answers
            .Select(answer =>
            {
                var answerText = answer.AnswerText.Trim();
                var normalized = answerText.ToLowerInvariant();
                var wordCount = answerText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
                var questionKeywords = ExtractKeywords(answer.QuestionText);
                var matchedKeywords = questionKeywords.Count(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                var keywordRatio = questionKeywords.Count == 0 ? 0.45m : (decimal)matchedKeywords / questionKeywords.Count;

                var completeness = ScoreCompleteness(wordCount, answer.MaxScore);
                var relevance = Math.Clamp(keywordRatio, 0.15m, 1m);
                var structure = ScoreStructure(answerText);
                var weighted = (completeness * 0.4m) + (relevance * 0.4m) + (structure * 0.2m);
                var suggestedScore = Math.Clamp((int)Math.Round(answer.MaxScore * weighted, MidpointRounding.AwayFromZero), 0, answer.MaxScore);
                var confidence = Math.Clamp(0.48m + (keywordRatio * 0.28m) + (Math.Min(wordCount, 120) / 120m * 0.18m), 0.35m, 0.92m);

                var breakdown = new List<AiRubricBreakdownItem>
                {
                    new("Completeness", ToCriterionScore(completeness), 5, BuildCompletenessComment(wordCount)),
                    new("Relevance", ToCriterionScore(relevance), 5, BuildRelevanceComment(matchedKeywords, questionKeywords.Count)),
                    new("Structure", ToCriterionScore(structure), 5, BuildStructureComment(answerText))
                };

                return new AiEssayGradeSuggestion(
                    answer.QuestionId,
                    suggestedScore,
                    Math.Round(confidence, 2),
                    BuildReasoning(suggestedScore, answer.MaxScore, matchedKeywords, questionKeywords.Count, wordCount),
                    breakdown);
            })
            .ToList();

        var tokenEstimate = request.Answers.Sum(a => EstimateTokens(a.QuestionText) + EstimateTokens(a.AnswerText) + 20);

        return Task.FromResult(new AiEssayGradingResult(
            "Local",
            config.UsesOpenAiCompatibleProvider() || string.IsNullOrWhiteSpace(config.Model)
                ? "local-essay-grader-v1"
                : config.Model,
            string.IsNullOrWhiteSpace(config.EssayGradingPromptVersion)
                ? "essay-grading-v1"
                : config.EssayGradingPromptVersion,
            tokenEstimate,
            suggestions));
    }

    private static decimal ScoreCompleteness(int wordCount, int maxScore)
    {
        var expectedWords = Math.Clamp(maxScore * 18, 45, 160);
        return Math.Clamp(wordCount / (decimal)expectedWords, 0.15m, 1m);
    }

    private static decimal ScoreStructure(string answer)
    {
        var score = 0.35m;
        if (answer.Contains('.', StringComparison.Ordinal)) score += 0.2m;
        if (answer.Contains("because", StringComparison.OrdinalIgnoreCase) ||
            answer.Contains("therefore", StringComparison.OrdinalIgnoreCase) ||
            answer.Contains("for example", StringComparison.OrdinalIgnoreCase))
            score += 0.25m;
        if (answer.Length >= 180) score += 0.2m;
        return Math.Clamp(score, 0.15m, 1m);
    }

    private static IReadOnlyList<string> ExtractKeywords(string text)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "with", "that", "this", "from", "your", "you", "are", "how", "what",
            "why", "when", "where", "explain", "describe", "discuss", "answer", "should", "could"
        };

        return text.Split([' ', ',', '.', ':', ';', '?', '!', '(', ')', '/', '-'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length >= 4 && !stopWords.Contains(x))
            .Distinct()
            .Take(10)
            .ToList();
    }

    private static int ToCriterionScore(decimal value) =>
        Math.Clamp((int)Math.Round(value * 5, MidpointRounding.AwayFromZero), 1, 5);

    private static string BuildCompletenessComment(int wordCount) =>
        wordCount < 35
            ? "The answer is brief and may miss supporting detail."
            : "The answer provides enough detail for a first-pass grading suggestion.";

    private static string BuildRelevanceComment(int matchedKeywords, int totalKeywords) =>
        totalKeywords == 0
            ? "The question has limited keyword signal, so relevance confidence is conservative."
            : $"The answer references {matchedKeywords} of {totalKeywords} key question terms.";

    private static string BuildStructureComment(string answer) =>
        answer.Contains("because", StringComparison.OrdinalIgnoreCase) ||
        answer.Contains("for example", StringComparison.OrdinalIgnoreCase)
            ? "The answer includes explanatory structure or examples."
            : "The answer would benefit from clearer reasoning or examples.";

    private static string BuildReasoning(int score, int maxScore, int matchedKeywords, int totalKeywords, int wordCount) =>
        $"Suggested {score}/{maxScore} based on answer completeness, {matchedKeywords}/{Math.Max(totalKeywords, 1)} matched key terms, and {wordCount} words of supporting detail.";

    private static int EstimateTokens(string text) =>
        Math.Max(1, (int)Math.Ceiling(text.Length / 4m));
}
