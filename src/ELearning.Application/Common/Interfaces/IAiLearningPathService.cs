namespace ELearning.Application.Common.Interfaces;

public interface IAiLearningPathService
{
    string CacheVariant { get; }
    Task<AiLearningPathDraft> GenerateAsync(AiLearningPathRequest request, CancellationToken ct = default);
}

public sealed record AiLearningPathRequest(
    string Goal,
    string? CurrentSkills,
    string? TargetRole,
    Guid? OrganizationId,
    int MaxCourses);

public sealed record AiLearningPathDraft(
    string Provider,
    string Model,
    string PromptVersion,
    int? TokenEstimate,
    string Goal,
    string? TargetRole,
    decimal Confidence,
    string EstimatedEffort,
    IReadOnlyList<string> MissingSkills,
    IReadOnlyList<AiLearningPathCourse> Courses);

public sealed record AiLearningPathCourse(
    int Order,
    Guid CourseId,
    string Title,
    string? Description,
    long PriceCents,
    string Currency,
    decimal Score,
    string EstimatedEffort,
    IReadOnlyList<string> Reasons);
