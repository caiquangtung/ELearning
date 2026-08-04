using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OpenAiCompatibleEssayGradingService(
    OpenAiCompatibleChatClient client,
    IOptions<AiOptions> options)
    : IAiEssayGradingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiEssayGradingResult> SuggestAsync(AiEssayGradingRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        var result = await client.CompleteJsonAsync(
            BuildSystemPrompt(),
            BuildUserPrompt(request),
            ct);

        var response = JsonSerializer.Deserialize<EssayGradingResponse>(
            OpenAiCompatibleJson.ExtractObject(result.Content),
            JsonOptions);

        if (response?.Suggestions is null || response.Suggestions.Count == 0)
            throw new InvalidOperationException("AI provider returned no grade suggestions.");

        var maxScores = request.Answers.ToDictionary(x => x.QuestionId, x => x.MaxScore);
        var suggestions = response.Suggestions.Select(item => ToSuggestion(item, maxScores)).ToList();

        return new AiEssayGradingResult(
            result.Provider,
            result.Model,
            string.IsNullOrWhiteSpace(config.EssayGradingPromptVersion)
                ? "essay-grading-v1"
                : config.EssayGradingPromptVersion,
            result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(
                request.QuizTitle,
                request.Rubric,
                string.Join(' ', request.Answers.Select(x => x.QuestionText + " " + x.AnswerText)),
                result.Content),
            suggestions);
    }

    private static AiEssayGradeSuggestion ToSuggestion(
        EssaySuggestionItem item,
        IReadOnlyDictionary<Guid, int> maxScores)
    {
        if (!Guid.TryParse(item.QuestionId, out var questionId) || !maxScores.TryGetValue(questionId, out var maxScore))
            throw new InvalidOperationException("AI provider returned a suggestion for an unknown question.");

        if (item.SuggestedScore < 0 || item.SuggestedScore > maxScore)
            throw new InvalidOperationException("AI provider returned a score outside the question bounds.");

        if (item.Confidence < 0 || item.Confidence > 1)
            throw new InvalidOperationException("AI provider returned confidence outside the 0-1 range.");

        var breakdown = (item.RubricBreakdown ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Criterion))
            .Select(x =>
            {
                var criterionMax = x.MaxScore <= 0 ? 5 : x.MaxScore;
                var criterionScore = Math.Clamp(x.Score, 0, criterionMax);
                return new AiRubricBreakdownItem(
                    x.Criterion!.Trim(),
                    criterionScore,
                    criterionMax,
                    string.IsNullOrWhiteSpace(x.Comment) ? "No additional comment." : x.Comment.Trim());
            })
            .ToList();

        if (breakdown.Count == 0)
        {
            breakdown.Add(new AiRubricBreakdownItem(
                "Overall",
                Math.Clamp(item.SuggestedScore, 0, Math.Max(maxScore, 1)),
                Math.Max(maxScore, 1),
                "AI provider returned an overall scoring suggestion."));
        }

        return new AiEssayGradeSuggestion(
            questionId,
            item.SuggestedScore,
            Math.Round(item.Confidence, 2),
            string.IsNullOrWhiteSpace(item.Reasoning)
                ? "AI provider returned a score suggestion without detailed reasoning."
                : item.Reasoning.Trim(),
            breakdown);
    }

    private static string BuildSystemPrompt() =>
        PromptTemplateStore.LoadSystemPrompt(
            "essay-grading-v1",
            """
            You are an LMS grading assistant. Return only a JSON object.
            Suggest grades but do not make final grading decisions.
            The JSON shape must be:
            {"suggestions":[{"questionId":"guid","suggestedScore":0,"confidence":0.0,"reasoning":"...","rubricBreakdown":[{"criterion":"...","score":0,"maxScore":5,"comment":"..."}]}]}
            suggestedScore must be between 0 and the provided maxScore. confidence must be between 0 and 1.
            """);

    private static string BuildUserPrompt(AiEssayGradingRequest request)
    {
        var payload = new
        {
            request.AttemptId,
            request.QuizId,
            request.QuizTitle,
            request.Rubric,
            Answers = request.Answers.Select(answer => new
            {
                answer.QuestionId,
                answer.QuestionText,
                answer.AnswerText,
                answer.MaxScore
            }).ToArray()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private sealed record EssayGradingResponse(IReadOnlyList<EssaySuggestionItem> Suggestions);

    private sealed record EssaySuggestionItem(
        string? QuestionId,
        int SuggestedScore,
        decimal Confidence,
        string? Reasoning,
        IReadOnlyList<EssayRubricItem>? RubricBreakdown);

    private sealed record EssayRubricItem(
        string? Criterion,
        int Score,
        int MaxScore,
        string? Comment);
}
