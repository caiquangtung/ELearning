using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.AssignInstructor;

public sealed record AssignInstructorCommand(Guid TrainingClassId, Guid UserId) : IRequest<Result>;
