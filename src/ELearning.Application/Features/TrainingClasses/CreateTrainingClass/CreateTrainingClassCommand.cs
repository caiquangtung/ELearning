using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.CreateTrainingClass;

public sealed record CreateTrainingClassCommand(Guid CourseId, string Title, int MaxLearners)
    : IRequest<Result<TrainingClassListItemDto>>;
