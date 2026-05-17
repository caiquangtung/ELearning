using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.SubmitAttempt;

public sealed record SubmitAttemptCommand(
    Guid AttemptId,
    Guid UserId,
    IReadOnlyList<AnswerSubmission> Answers)
    : IRequest<Result<QuizAttemptDto>>;

public sealed record AnswerSubmission(Guid QuestionId, Guid? SelectedOptionId, string? TextAnswer);
