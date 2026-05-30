namespace ELearning.Application.Common.Interfaces;

public interface IAiSemanticSearchService
{
    Task<IReadOnlyList<AiSemanticCourseSearchResult>> SearchCoursesAsync(
        string query,
        int limit,
        CancellationToken ct = default);
}

public sealed record AiSemanticCourseSearchResult(
    Guid CourseId,
    string Title,
    string? Description,
    long PriceCents,
    string Currency,
    DateTime CreatedAt,
    decimal Score,
    IReadOnlyList<string> MatchedConcepts,
    IReadOnlyList<string> Reasons);
