using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.CancelSession;

public sealed class CancelSessionCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelSessionCommand, Result>
{
    public async Task<Result> Handle(CancelSessionCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdWithDetailsAsync(request.TrainingClassId, ct);
        if (trainingClass is null)
            return Result.Failure(Error.NotFound("TrainingClass", request.TrainingClassId));

        var session = trainingClass.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
        if (session is null)
            return Result.Failure(Error.NotFound("ClassSession", request.SessionId));

        session.Cancel();

        trainingClassRepository.Update(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
