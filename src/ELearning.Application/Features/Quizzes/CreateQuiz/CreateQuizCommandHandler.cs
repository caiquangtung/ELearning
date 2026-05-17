using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQuizCommand, Result<QuizDto>>
{
    public async Task<Result<QuizDto>> Handle(CreateQuizCommand request, CancellationToken ct)
    {
        Quiz quiz;
        if (request.LessonId.HasValue)
        {
            quiz = Quiz.CreateForLesson(request.LessonId.Value, request.Title, request.Description, request.TimeLimitMinutes, request.PassingScore);
        }
        else if (request.CourseId.HasValue)
        {
            quiz = Quiz.CreateForCourse(request.CourseId.Value, request.Title, request.Description, request.TimeLimitMinutes, request.PassingScore);
        }
        else
        {
            return Result.Failure<QuizDto>(Error.Validation("Quiz", "Either CourseId or LessonId must be provided."));
        }

        quizRepository.Add(quiz);
        await unitOfWork.SaveChangesAsync(ct);

        return new QuizDto(
            quiz.Id,
            quiz.CourseId,
            quiz.LessonId,
            quiz.Title,
            quiz.Description,
            quiz.Status.ToString(),
            quiz.TimeLimitMinutes,
            quiz.PassingScore,
            quiz.CreatedAt,
            quiz.UpdatedAt);
    }
}
