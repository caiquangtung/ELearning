using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GradeAttempt;

public sealed class GradeAttemptCommandHandler(
    IQuizAttemptRepository attemptRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<GradeAttemptCommand, Result<QuizAttemptDto>>
{
    public async Task<Result<QuizAttemptDto>> Handle(GradeAttemptCommand request, CancellationToken ct)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, ct);

        if (attempt is null)
            return Result.Failure<QuizAttemptDto>(Error.NotFound("QuizAttempt", request.AttemptId));

        if (attempt.Status != AttemptStatus.Submitted)
            return Result.Failure<QuizAttemptDto>(Error.Validation("QuizAttempt", "Attempt must be submitted before grading."));

        var gradedBy = currentUserService.UserId?.ToString() ?? "system";
        int totalScore = 0;

        foreach (var grade in request.Grades)
        {
            var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == grade.QuestionId);
            if (answer is null)
                continue;

            answer.Grade(grade.Score, grade.IsCorrect, gradedBy);
            totalScore += grade.Score;
        }

        // Recalculate total score from all answers
        totalScore = attempt.Answers.Where(a => a.Score.HasValue).Sum(a => a.Score!.Value);
        attempt.SetScore(totalScore);

        attemptRepository.Update(attempt);
        await unitOfWork.SaveChangesAsync(ct);

        return new QuizAttemptDto(
            attempt.Id,
            attempt.QuizId,
            attempt.UserId,
            attempt.StartedAt,
            attempt.SubmittedAt,
            attempt.Status.ToString(),
            attempt.TotalScore,
            attempt.CreatedAt);
    }
}
