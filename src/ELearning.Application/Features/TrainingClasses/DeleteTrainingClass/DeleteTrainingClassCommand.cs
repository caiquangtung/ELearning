using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.DeleteTrainingClass;

public sealed record DeleteTrainingClassCommand(Guid Id) : IRequest<Result>;
