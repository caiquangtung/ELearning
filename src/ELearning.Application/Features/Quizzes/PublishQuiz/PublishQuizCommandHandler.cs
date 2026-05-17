using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.PublishQuiz;

public sealed class PublishQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PublishQuizCommand, Result<QuizDto>>
{
    public async Task<Result<QuizDto>> Handle(PublishQuizCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdWithQuestionsAsync(request.Id, ct);
        if (quiz is null)
            return Result.Failure<QuizDto>(Error.NotFound("Quiz", request.Id));

        try
        {
            quiz.Publish();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<QuizDto>(Error.Validation("Quiz.Publish", ex.Message));
        }

        quizRepository.Update(quiz);
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
