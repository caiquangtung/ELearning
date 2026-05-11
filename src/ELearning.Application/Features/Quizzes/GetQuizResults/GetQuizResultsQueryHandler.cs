using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuizResults;

public sealed class GetQuizResultsQueryHandler(
    IQuizAttemptRepository attemptRepository,
    IQuizRepository quizRepository)
    : IRequestHandler<GetQuizResultsQuery, Result<QuizResultDto>>
{
    public async Task<Result<QuizResultDto>> Handle(GetQuizResultsQuery request, CancellationToken ct)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, ct);

        if (attempt is null)
            return Result.Failure<QuizResultDto>(Error.NotFound("QuizAttempt", request.AttemptId));

        if (attempt.UserId != request.UserId)
            return Result.Failure<QuizResultDto>(Error.Unauthorized("You do not own this attempt."));

        if (attempt.Status != AttemptStatus.Submitted && attempt.Status != AttemptStatus.Graded)
            return Result.Failure<QuizResultDto>(Error.Validation("QuizAttempt", "Results are not available yet."));

        var quiz = await quizRepository.GetByIdAsync(attempt.QuizId, ct);

        var quizTitle = quiz?.Title ?? "Unknown";
        var passingScore = quiz?.PassingScore;
        bool passed = attempt.TotalScore.HasValue && passingScore.HasValue && attempt.TotalScore.Value >= passingScore.Value;

        var questionResults = attempt.Answers.Select(a => new QuestionResultDto(
            a.QuestionId,
            "", // Question text not stored on answer; could be enriched with a join
            0,  // Points not stored on answer
            a.Score,
            a.IsCorrect,
            a.TextAnswer,
            a.SelectedOptionId)).ToList();

        return new QuizResultDto(
            attempt.Id,
            attempt.QuizId,
            quizTitle,
            attempt.TotalScore,
            passingScore,
            passed,
            attempt.SubmittedAt ?? attempt.CreatedAt,
            questionResults);
    }
}
