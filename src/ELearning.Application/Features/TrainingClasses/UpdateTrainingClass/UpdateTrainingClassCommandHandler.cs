using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.UpdateTrainingClass;

public sealed class UpdateTrainingClassCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTrainingClassCommand, Result<TrainingClassListItemDto>>
{
    public async Task<Result<TrainingClassListItemDto>> Handle(UpdateTrainingClassCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdAsync(request.Id, ct);
        if (trainingClass is null)
            return Result.Failure<TrainingClassListItemDto>(Error.NotFound("TrainingClass", request.Id));

        trainingClass.Update(request.Title, request.MaxLearners);
        trainingClassRepository.Update(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return new TrainingClassListItemDto(
            trainingClass.Id,
            trainingClass.CourseId,
            trainingClass.Title,
            trainingClass.Status.ToString(),
            trainingClass.MaxLearners,
            trainingClass.CreatedAt);
    }
}
