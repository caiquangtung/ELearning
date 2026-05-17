using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateQuizCommand, Result<QuizDto>>
{
    public async Task<Result<QuizDto>> Handle(UpdateQuizCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdAsync(request.Id, ct);
        if (quiz is null)
            return Result.Failure<QuizDto>(Error.NotFound("Quiz", request.Id));

        quiz.Update(request.Title, request.Description, request.TimeLimitMinutes, request.PassingScore);
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
