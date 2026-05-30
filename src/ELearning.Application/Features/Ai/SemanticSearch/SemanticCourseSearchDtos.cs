namespace ELearning.Application.Features.Ai.SemanticSearch;

public sealed record SemanticCourseSearchDto(
    string Provider,
    string Model,
    string PromptVersion,
    string InputHash,
    IReadOnlyList<SemanticCourseSearchResultDto> Results);

public sealed record SemanticCourseSearchResultDto(
    Guid CourseId,
    string Title,
    string? Description,
    long PriceCents,
    string Currency,
    DateTime CreatedAt,
    decimal Score,
    IReadOnlyList<string> MatchedConcepts,
    IReadOnlyList<string> Reasons);
