namespace ELearning.Application.Features.Quizzes.Common;

public sealed record QuizDto(
    Guid Id,
    Guid? CourseId,
    Guid? LessonId,
    string Title,
    string? Description,
    string Status,
    int? TimeLimitMinutes,
    int? PassingScore,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record QuizListItemDto(
    Guid Id,
    string Title,
    string Status,
    int QuestionCount,
    DateTime CreatedAt);

public sealed record QuizDetailDto(
    Guid Id,
    Guid? CourseId,
    Guid? LessonId,
    string Title,
    string? Description,
    string Status,
    int? TimeLimitMinutes,
    int? PassingScore,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<QuestionDto> Questions);

public sealed record QuestionDto(
    Guid Id,
    string Text,
    string Type,
    int Points,
    int SortOrder,
    IReadOnlyList<QuestionOptionDto> Options);

public sealed record QuestionOptionDto(
    Guid Id,
    string Text,
    bool IsCorrect,
    int SortOrder);

public sealed record QuizAttemptDto(
    Guid Id,
    Guid QuizId,
    Guid UserId,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    string Status,
    int? TotalScore,
    DateTime CreatedAt);

public sealed record QuizResultDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    int? TotalScore,
    int? PassingScore,
    bool Passed,
    DateTime SubmittedAt,
    IReadOnlyList<QuestionResultDto> QuestionResults);

public sealed record QuestionResultDto(
    Guid QuestionId,
    string QuestionText,
    int Points,
    int? Score,
    bool? IsCorrect,
    string? TextAnswer,
    Guid? SelectedOptionId);

public sealed record QuizAnalyticsDto(
    Guid QuizId,
    string QuizTitle,
    int TotalAttempts,
    int CompletedAttempts,
    double AverageScore,
    double PassRate,
    int HighestScore,
    int LowestScore);
