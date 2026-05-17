using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.StartAttempt;

public sealed record StartAttemptCommand(Guid QuizId, Guid UserId)
    : IRequest<Result<QuizAttemptDto>>;
