using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.SubmitAttempt;

public sealed class SubmitAttemptCommandHandler(
    IQuizRepository quizRepository,
    IQuizAttemptRepository attemptRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitAttemptCommand, Result<QuizAttemptDto>>
{
    public async Task<Result<QuizAttemptDto>> Handle(SubmitAttemptCommand request, CancellationToken ct)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, ct);

        if (attempt is null)
            return Result.Failure<QuizAttemptDto>(Error.NotFound("QuizAttempt", request.AttemptId));

        if (attempt.UserId != request.UserId)
            return Result.Failure<QuizAttemptDto>(Error.Unauthorized("You do not own this attempt."));

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<QuizAttemptDto>(Error.Validation("QuizAttempt", "Attempt is not in progress."));

        var quiz = await quizRepository.GetByIdWithQuestionsAsync(attempt.QuizId, ct);
        if (quiz is null)
            return Result.Failure<QuizAttemptDto>(Error.NotFound("Quiz", attempt.QuizId));

        foreach (var answer in request.Answers)
        {
            attempt.AddAnswer(answer.QuestionId, answer.SelectedOptionId, answer.TextAnswer);
        }

        try
        {
            attempt.Submit();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<QuizAttemptDto>(Error.Validation("QuizAttempt.Submit", ex.Message));
        }

        // Auto-grade multiple choice
        int totalScore = 0;
        foreach (var answer in attempt.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId && !q.IsDeleted);
            if (question is null) continue;

            if (question.Type == QuestionType.MultipleChoice && answer.SelectedOptionId.HasValue)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId.Value && !o.IsDeleted);
                if (option is not null)
                {
                    bool isCorrect = option.IsCorrect;
                    int score = isCorrect ? question.Points : 0;
                    answer.Grade(score, isCorrect, "system");
                    totalScore += score;
                }
                else
                {
                    answer.Grade(0, false, "system");
                }
            }
        }

        if (attempt.Answers.Any(a => a.Score.HasValue))
        {
            attempt.SetScore(totalScore);
        }

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
