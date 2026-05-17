using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.StartAttempt;

public sealed class StartAttemptCommandHandler(
    IQuizRepository quizRepository,
    IQuizAttemptRepository attemptRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartAttemptCommand, Result<QuizAttemptDto>>
{
    public async Task<Result<QuizAttemptDto>> Handle(StartAttemptCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdAsync(request.QuizId, ct);
        if (quiz is null)
            return Result.Failure<QuizAttemptDto>(Error.NotFound("Quiz", request.QuizId));

        if (quiz.Status != QuizStatus.Published)
            return Result.Failure<QuizAttemptDto>(Error.Validation("Quiz.Attempt", "Quiz is not published."));

        var existingInProgress = await attemptRepository.GetInProgressAsync(request.QuizId, request.UserId, ct);

        if (existingInProgress is not null)
        {
            return new QuizAttemptDto(
                existingInProgress.Id,
                existingInProgress.QuizId,
                existingInProgress.UserId,
                existingInProgress.StartedAt,
                existingInProgress.SubmittedAt,
                existingInProgress.Status.ToString(),
                existingInProgress.TotalScore,
                existingInProgress.CreatedAt);
        }

        var attempt = QuizAttempt.Start(request.QuizId, request.UserId);
        attemptRepository.Add(attempt);
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
