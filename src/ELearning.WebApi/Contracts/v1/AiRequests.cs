namespace ELearning.WebApi.Contracts.v1;

public sealed record GenerateQuizQuestionsRequest(
    Guid CourseId,
    Guid? LessonId,
    int QuestionCount,
    string Difficulty,
    IReadOnlyList<string> QuestionTypes);
