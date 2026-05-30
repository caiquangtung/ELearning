namespace ELearning.Application.Common.Interfaces;

public interface IAiCourseRecommendationService
{
    Task<IReadOnlyList<AiCourseRecommendationCandidate>> RecommendAsync(
        Guid userId,
        int limit,
        CancellationToken ct = default);
}

public sealed record AiCourseRecommendationCandidate(
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
