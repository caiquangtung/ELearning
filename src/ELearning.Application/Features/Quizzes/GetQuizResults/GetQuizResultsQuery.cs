using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuizResults;

public sealed record GetQuizResultsQuery(Guid AttemptId, Guid UserId)
    : IRequest<Result<QuizResultDto>>;
