namespace ELearning.Application.Common.Interfaces;

public interface IAiQuizQuestionGenerator
{
    Task<AiQuizQuestionGenerationResult> GenerateAsync(AiQuizQuestionGenerationRequest request, CancellationToken ct = default);
}

public sealed record AiQuizQuestionGenerationRequest(
    Guid CourseId,
    Guid? LessonId,
    string CourseTitle,
    string? CourseDescription,
    string? LessonTitle,
    string? LessonContent,
    int QuestionCount,
    string Difficulty,
    IReadOnlyList<string> QuestionTypes);

public sealed record AiQuizQuestionGenerationResult(
    string Provider,
    string Model,
    string PromptVersion,
    int TokenEstimate,
    IReadOnlyList<AiGeneratedQuestion> Questions);

public sealed record AiGeneratedQuestion(
    string Text,
    string Type,
    int Points,
    int SortOrder,
    string Difficulty,
    string Explanation,
    IReadOnlyList<AiGeneratedQuestionOption> Options);

public sealed record AiGeneratedQuestionOption(string Text, bool IsCorrect, int SortOrder);
