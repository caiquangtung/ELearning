using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.RemoveInstructor;

public sealed record RemoveInstructorCommand(Guid TrainingClassId, Guid UserId) : IRequest<Result>;
