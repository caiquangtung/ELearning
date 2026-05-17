using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.RemoveQuestion;

public sealed class RemoveQuestionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveQuestionCommand, Result>
{
    public async Task<Result> Handle(RemoveQuestionCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdWithQuestionsAsync(request.QuizId, ct);
        if (quiz is null)
            return Result.Failure(Error.NotFound("Quiz", request.QuizId));

        try
        {
            quiz.RemoveQuestion(request.QuestionId);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(Error.Validation("Quiz.RemoveQuestion", ex.Message));
        }

        quizRepository.Update(quiz);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
