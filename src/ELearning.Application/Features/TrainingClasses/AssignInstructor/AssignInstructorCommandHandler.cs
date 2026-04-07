using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.AssignInstructor;

public sealed class AssignInstructorCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignInstructorCommand, Result>
{
    public async Task<Result> Handle(AssignInstructorCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdWithDetailsAsync(request.TrainingClassId, ct);
        if (trainingClass is null)
            return Result.Failure(Error.NotFound("TrainingClass", request.TrainingClassId));

        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User", request.UserId));

        if (!user.HasRole(Roles.Instructor) && !user.HasRole(Roles.Admin))
            return Result.Failure(
                Error.Validation("UserId", "User must have Instructor or Admin role to be assigned as an instructor."));

        trainingClass.AddInstructor(request.UserId);
        trainingClassRepository.Update(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
