using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GradeAttempt;

public sealed record GradeAttemptCommand(
    Guid AttemptId,
    IReadOnlyList<QuestionGrade> Grades)
    : IRequest<Result<QuizAttemptDto>>;

public sealed record QuestionGrade(Guid QuestionId, int Score, bool? IsCorrect);
