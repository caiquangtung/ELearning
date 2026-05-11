using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuiz;

public sealed class GetQuizQueryHandler(IQuizRepository quizRepository)
    : IRequestHandler<GetQuizQuery, Result<QuizDetailDto>>
{
    public async Task<Result<QuizDetailDto>> Handle(GetQuizQuery request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdWithQuestionsAsync(request.Id, ct);
        if (quiz is null)
            return Result.Failure<QuizDetailDto>(Error.NotFound("Quiz", request.Id));

        var dto = new QuizDetailDto(
            quiz.Id,
            quiz.CourseId,
            quiz.LessonId,
            quiz.Title,
            quiz.Description,
            quiz.Status.ToString(),
            quiz.TimeLimitMinutes,
            quiz.PassingScore,
            quiz.CreatedAt,
            quiz.UpdatedAt,
            quiz.Questions
                .Where(q => !q.IsDeleted)
                .OrderBy(q => q.SortOrder)
                .Select(q => new QuestionDto(
                    q.Id,
                    q.Text,
                    q.Type.ToString(),
                    q.Points,
                    q.SortOrder,
                    q.Options
                        .Where(o => !o.IsDeleted)
                        .OrderBy(o => o.SortOrder)
                        .Select(o => new QuestionOptionDto(o.Id, o.Text, o.IsCorrect, o.SortOrder))
                        .ToList()))
                .ToList());

        return dto;
    }
}
