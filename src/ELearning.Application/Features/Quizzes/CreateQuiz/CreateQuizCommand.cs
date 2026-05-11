using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.CreateQuiz;

public sealed record CreateQuizCommand(
    Guid? CourseId,
    Guid? LessonId,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? PassingScore)
    : IRequest<Result<QuizDto>>;
