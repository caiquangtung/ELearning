using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.QuizQuestionGeneration;

public sealed class GenerateQuizQuestionsCommandHandler(
    ICourseRepository courseRepository,
    IAiQuizQuestionGenerator generator,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GenerateQuizQuestionsCommand, Result<GeneratedQuizQuestionsDto>>
{
    public async Task<Result<GeneratedQuizQuestionsDto>> Handle(GenerateQuizQuestionsCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<GeneratedQuizQuestionsDto>(Error.NotFound("Course", request.CourseId));

        var lesson = request.LessonId.HasValue
            ? course.Sections.SelectMany(s => s.Lessons).FirstOrDefault(l => l.Id == request.LessonId.Value)
            : null;

        if (request.LessonId.HasValue && lesson is null)
            return Result.Failure<GeneratedQuizQuestionsDto>(Error.NotFound("Lesson", request.LessonId.Value));

        var input = new AiQuizQuestionGenerationRequest(
            course.Id,
            lesson?.Id,
            course.Title,
            course.Description,
            lesson?.Title,
            lesson?.Content,
            request.QuestionCount,
            NormalizeDifficulty(request.Difficulty),
            request.QuestionTypes.Select(NormalizeQuestionType).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        var inputHash = ComputeInputHash(input);

        try
        {
            var generated = await generator.GenerateAsync(input, ct);

            if (generated.Questions.Count == 0)
                return await LogFailure("AI provider returned no questions.");

            var invalid = generated.Questions.FirstOrDefault(q =>
                string.IsNullOrWhiteSpace(q.Text) ||
                string.IsNullOrWhiteSpace(q.Type) ||
                (q.Type.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase) && q.Options.Count < 2));

            if (invalid is not null)
                return await LogFailure("AI provider returned an invalid question payload.");

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "QuizQuestionGeneration",
                generated.Provider,
                generated.Model,
                generated.PromptVersion,
                inputHash,
                generated.TokenEstimate));
            await unitOfWork.SaveChangesAsync(ct);

            return new GeneratedQuizQuestionsDto(
                course.Id,
                lesson?.Id,
                generated.Provider,
                generated.Model,
                generated.PromptVersion,
                inputHash,
                generated.Questions.Select(q => new GeneratedQuizQuestionDto(
                    q.Text,
                    NormalizeQuestionType(q.Type),
                    q.Points,
                    q.SortOrder,
                    q.Difficulty,
                    q.Explanation,
                    q.Options.Select(o => new GeneratedQuizQuestionOptionDto(o.Text, o.IsCorrect, o.SortOrder)).ToList()))
                    .ToList());
        }
        catch (Exception ex)
        {
            return await LogFailure(ex.Message);
        }

        async Task<Result<GeneratedQuizQuestionsDto>> LogFailure(string message)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "QuizQuestionGeneration",
                "AI",
                "unknown",
                "quiz-question-generator-v1",
                inputHash,
                message));
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Failure<GeneratedQuizQuestionsDto>(Error.Validation("AI.Generation", message));
        }
    }

    private static string NormalizeDifficulty(string difficulty) =>
        difficulty.Trim().ToLowerInvariant() switch
        {
            "easy" => "Easy",
            "hard" => "Hard",
            _ => "Medium"
        };

    private static string NormalizeQuestionType(string type) =>
        type.Trim().ToLowerInvariant() switch
        {
            "essay" => "Essay",
            "code" => "Code",
            _ => "MultipleChoice"
        };

    private static string ComputeInputHash(AiQuizQuestionGenerationRequest input)
    {
        var raw = string.Join('|',
            input.CourseId,
            input.LessonId,
            input.CourseTitle,
            input.CourseDescription,
            input.LessonTitle,
            input.LessonContent,
            input.QuestionCount,
            input.Difficulty,
            string.Join(',', input.QuestionTypes));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
