using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.DeleteQuiz;

public sealed class DeleteQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQuizCommand, Result>
{
    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdAsync(request.Id, ct);
        if (quiz is null)
            return Result.Failure(Error.NotFound("Quiz", request.Id));

        quizRepository.Remove(quiz);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
