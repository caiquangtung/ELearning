namespace ELearning.Application.Features.Ai.QuizQuestionGeneration;

public sealed record GeneratedQuizQuestionsDto(
    Guid CourseId,
    Guid? LessonId,
    string Provider,
    string Model,
    string PromptVersion,
    string InputHash,
    IReadOnlyList<GeneratedQuizQuestionDto> Questions);

public sealed record GeneratedQuizQuestionDto(
    string Text,
    string Type,
    int Points,
    int SortOrder,
    string Difficulty,
    string Explanation,
    IReadOnlyList<GeneratedQuizQuestionOptionDto> Options);

public sealed record GeneratedQuizQuestionOptionDto(string Text, bool IsCorrect, int SortOrder);
