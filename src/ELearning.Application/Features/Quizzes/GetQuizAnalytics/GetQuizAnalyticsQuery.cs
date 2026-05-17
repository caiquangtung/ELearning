using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuizAnalytics;

public sealed record GetQuizAnalyticsQuery(Guid QuizId)
    : IRequest<Result<QuizAnalyticsDto>>;
