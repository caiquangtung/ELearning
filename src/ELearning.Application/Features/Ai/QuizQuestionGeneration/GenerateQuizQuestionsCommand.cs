using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.QuizQuestionGeneration;

public sealed record GenerateQuizQuestionsCommand(
    Guid CourseId,
    Guid? LessonId,
    int QuestionCount,
    string Difficulty,
    IReadOnlyList<string> QuestionTypes)
    : IRequest<Result<GeneratedQuizQuestionsDto>>;
