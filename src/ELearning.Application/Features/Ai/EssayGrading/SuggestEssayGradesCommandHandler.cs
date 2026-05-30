using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.EssayGrading;

public sealed class SuggestEssayGradesCommandHandler(
    IQuizAttemptRepository attemptRepository,
    IQuizRepository quizRepository,
    IAiEssayGradingService gradingService,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SuggestEssayGradesCommand, Result<EssayGradeSuggestionsDto>>
{
    public async Task<Result<EssayGradeSuggestionsDto>> Handle(SuggestEssayGradesCommand request, CancellationToken ct)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, ct);
        if (attempt is null)
            return Result.Failure<EssayGradeSuggestionsDto>(Error.NotFound("QuizAttempt", request.AttemptId));

        if (attempt.Status != AttemptStatus.Submitted)
            return Result.Failure<EssayGradeSuggestionsDto>(Error.Validation("QuizAttempt", "Attempt must be submitted before AI grading suggestions."));

        var quiz = await quizRepository.GetByIdWithQuestionsAsync(attempt.QuizId, ct);
        if (quiz is null)
            return Result.Failure<EssayGradeSuggestionsDto>(Error.NotFound("Quiz", attempt.QuizId));

        var questionMap = quiz.Questions
            .Where(q => !q.IsDeleted && (q.Type == QuestionType.Essay || q.Type == QuestionType.Code))
            .ToDictionary(q => q.Id);

        var answerInputs = attempt.Answers
            .Where(a => questionMap.ContainsKey(a.QuestionId) && !string.IsNullOrWhiteSpace(a.TextAnswer))
            .Select(a =>
            {
                var question = questionMap[a.QuestionId];
                return new AiEssayAnswerInput(
                    a.QuestionId,
                    question.Text,
                    a.TextAnswer!,
                    question.Points);
            })
            .ToList();

        if (answerInputs.Count == 0)
            return Result.Failure<EssayGradeSuggestionsDto>(Error.Validation("AI.Grading", "No essay or code answers are available for AI grading suggestions."));

        var input = new AiEssayGradingRequest(
            attempt.Id,
            quiz.Id,
            quiz.Title,
            answerInputs,
            string.IsNullOrWhiteSpace(request.Rubric) ? null : request.Rubric.Trim());

        var inputHash = ComputeInputHash(input);

        try
        {
            var result = await gradingService.SuggestAsync(input, ct);
            if (result.Suggestions.Count == 0)
                return await LogFailure("AI provider returned no grade suggestions.");

            var suggestionsByQuestion = result.Suggestions.ToDictionary(x => x.QuestionId);
            var invalid = answerInputs.Any(answer =>
                !suggestionsByQuestion.TryGetValue(answer.QuestionId, out var suggestion) ||
                suggestion.SuggestedScore < 0 ||
                suggestion.SuggestedScore > answer.MaxScore ||
                suggestion.Confidence < 0 ||
                suggestion.Confidence > 1);

            if (invalid)
                return await LogFailure("AI provider returned invalid grade suggestions.");

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "EssayGradingSuggestion",
                result.Provider,
                result.Model,
                result.PromptVersion,
                inputHash,
                result.TokenEstimate));
            await unitOfWork.SaveChangesAsync(ct);

            return new EssayGradeSuggestionsDto(
                attempt.Id,
                result.Provider,
                result.Model,
                result.PromptVersion,
                inputHash,
                answerInputs.Select(answer =>
                {
                    var suggestion = suggestionsByQuestion[answer.QuestionId];
                    return new EssayGradeSuggestionDto(
                        answer.QuestionId,
                        answer.QuestionText,
                        answer.AnswerText,
                        answer.MaxScore,
                        suggestion.SuggestedScore,
                        suggestion.Confidence,
                        suggestion.Reasoning,
                        suggestion.RubricBreakdown.Select(item => new EssayRubricBreakdownItemDto(
                            item.Criterion,
                            item.Score,
                            item.MaxScore,
                            item.Comment)).ToList());
                }).ToList());
        }
        catch (Exception ex)
        {
            return await LogFailure(ex.Message);
        }

        async Task<Result<EssayGradeSuggestionsDto>> LogFailure(string message)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "EssayGradingSuggestion",
                "Local",
                "local-essay-grader-v1",
                "essay-grading-v1",
                inputHash,
                message));
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Failure<EssayGradeSuggestionsDto>(Error.Validation("AI.Grading", message));
        }
    }

    private static string ComputeInputHash(AiEssayGradingRequest input)
    {
        var raw = string.Join('|',
            input.AttemptId,
            input.QuizId,
            input.QuizTitle,
            input.Rubric,
            string.Join(";", input.Answers.Select(a => $"{a.QuestionId}:{a.QuestionText}:{a.AnswerText}:{a.MaxScore}")));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
