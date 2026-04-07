using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.UpdateTrainingClass;

public sealed record UpdateTrainingClassCommand(Guid Id, string Title, int MaxLearners)
    : IRequest<Result<TrainingClassListItemDto>>;
