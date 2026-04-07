using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.GetTrainingClass;

public sealed record GetTrainingClassQuery(Guid Id) : IRequest<Result<TrainingClassDetailDto>>;
