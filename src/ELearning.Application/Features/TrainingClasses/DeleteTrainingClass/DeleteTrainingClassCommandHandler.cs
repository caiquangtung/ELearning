using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.DeleteTrainingClass;

public sealed class DeleteTrainingClassCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTrainingClassCommand, Result>
{
    public async Task<Result> Handle(DeleteTrainingClassCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdAsync(request.Id, ct);
        if (trainingClass is null)
            return Result.Failure(Error.NotFound("TrainingClass", request.Id));

        trainingClassRepository.Remove(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
