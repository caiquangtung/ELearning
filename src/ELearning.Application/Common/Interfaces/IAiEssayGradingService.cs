namespace ELearning.Application.Common.Interfaces;

public interface IAiEssayGradingService
{
    Task<AiEssayGradingResult> SuggestAsync(AiEssayGradingRequest request, CancellationToken ct = default);
}

public sealed record AiEssayGradingRequest(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    IReadOnlyList<AiEssayAnswerInput> Answers,
    string? Rubric);

public sealed record AiEssayAnswerInput(
    Guid QuestionId,
    string QuestionText,
    string AnswerText,
    int MaxScore);

public sealed record AiEssayGradingResult(
    string Provider,
    string Model,
    string PromptVersion,
    int? TokenEstimate,
    IReadOnlyList<AiEssayGradeSuggestion> Suggestions);

public sealed record AiEssayGradeSuggestion(
    Guid QuestionId,
    int SuggestedScore,
    decimal Confidence,
    string Reasoning,
    IReadOnlyList<AiRubricBreakdownItem> RubricBreakdown);

public sealed record AiRubricBreakdownItem(
    string Criterion,
    int Score,
    int MaxScore,
    string Comment);
