namespace ELearning.Application.Features.Ai.CourseRecommendations;

public sealed record CourseRecommendationsDto(
    string Provider,
    string Model,
    string PromptVersion,
    string InputHash,
    IReadOnlyList<CourseRecommendationDto> Items);

public sealed record CourseRecommendationDto(
    Guid CourseId,
    string Title,
    string? Description,
    long PriceCents,
    string Currency,
    DateTime CreatedAt,
    decimal Score,
    bool IsFallback,
    IReadOnlyList<string> Reasons,
    IReadOnlyDictionary<string, decimal> Signals);
