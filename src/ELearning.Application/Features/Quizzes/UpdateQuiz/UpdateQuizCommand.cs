using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.UpdateQuiz;

public sealed record UpdateQuizCommand(
    Guid Id,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? PassingScore)
    : IRequest<Result<QuizDto>>;
