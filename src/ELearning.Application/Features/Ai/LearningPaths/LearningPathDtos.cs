namespace ELearning.Application.Features.Ai.LearningPaths;

public sealed record GenerateLearningPathRequestDto(
    string Goal,
    string? CurrentSkills,
    string? TargetRole,
    Guid? OrganizationId,
    int MaxCourses);

public sealed record LearningPathDraftDto(
    string Provider,
    string Model,
    string PromptVersion,
    string InputHash,
    string Goal,
    string? TargetRole,
    decimal Confidence,
    string EstimatedEffort,
    IReadOnlyList<string> MissingSkills,
    IReadOnlyList<LearningPathCourseDto> Courses);

public sealed record LearningPathCourseDto(
    int Order,
    Guid CourseId,
    string Title,
    string? Description,
    long PriceCents,
    string Currency,
    decimal Score,
    string EstimatedEffort,
    IReadOnlyList<string> Reasons);
