using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuizAnalytics;

public sealed class GetQuizAnalyticsQueryHandler(
    IQuizRepository quizRepository,
    IQuizAttemptRepository attemptRepository)
    : IRequestHandler<GetQuizAnalyticsQuery, Result<QuizAnalyticsDto>>
{
    public async Task<Result<QuizAnalyticsDto>> Handle(GetQuizAnalyticsQuery request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdAsync(request.QuizId, ct);
        if (quiz is null)
            return Result.Failure<QuizAnalyticsDto>(Error.NotFound("Quiz", request.QuizId));

        var attempts = await attemptRepository.ListByQuizAsync(request.QuizId, ct);

        var totalAttempts = attempts.Count;
        var completed = attempts.Where(a => a.Status == AttemptStatus.Submitted || a.Status == AttemptStatus.Graded).ToList();
        var completedAttempts = completed.Count;
        var scored = completed.Where(a => a.TotalScore.HasValue).ToList();

        double averageScore = scored.Any() ? scored.Average(a => a.TotalScore!.Value) : 0;
        int highestScore = scored.Any() ? scored.Max(a => a.TotalScore!.Value) : 0;
        int lowestScore = scored.Any() ? scored.Min(a => a.TotalScore!.Value) : 0;

        double passRate = 0;
        if (scored.Any() && quiz.PassingScore.HasValue)
        {
            var passedCount = scored.Count(a => a.TotalScore!.Value >= quiz.PassingScore.Value);
            passRate = (double)passedCount / scored.Count * 100;
        }

        return new QuizAnalyticsDto(
            quiz.Id,
            quiz.Title,
            totalAttempts,
            completedAttempts,
            averageScore,
            passRate,
            highestScore,
            lowestScore);
    }
}
