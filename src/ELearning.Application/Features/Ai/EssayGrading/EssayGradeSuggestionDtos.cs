namespace ELearning.Application.Features.Ai.EssayGrading;

public sealed record EssayGradeSuggestionsDto(
    Guid AttemptId,
    string Provider,
    string Model,
    string PromptVersion,
    string InputHash,
    IReadOnlyList<EssayGradeSuggestionDto> Suggestions);

public sealed record EssayGradeSuggestionDto(
    Guid QuestionId,
    string QuestionText,
    string AnswerText,
    int MaxScore,
    int SuggestedScore,
    decimal Confidence,
    string Reasoning,
    IReadOnlyList<EssayRubricBreakdownItemDto> RubricBreakdown);

public sealed record EssayRubricBreakdownItemDto(
    string Criterion,
    int Score,
    int MaxScore,
    string Comment);
