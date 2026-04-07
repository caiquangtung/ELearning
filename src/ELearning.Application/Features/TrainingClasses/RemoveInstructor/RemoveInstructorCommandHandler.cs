using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.RemoveInstructor;

public sealed class RemoveInstructorCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveInstructorCommand, Result>
{
    public async Task<Result> Handle(RemoveInstructorCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdWithDetailsAsync(request.TrainingClassId, ct);
        if (trainingClass is null)
            return Result.Failure(Error.NotFound("TrainingClass", request.TrainingClassId));

        try
        {
            trainingClass.RemoveInstructor(request.UserId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(Error.Validation("UserId", ex.Message));
        }

        trainingClassRepository.Update(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
