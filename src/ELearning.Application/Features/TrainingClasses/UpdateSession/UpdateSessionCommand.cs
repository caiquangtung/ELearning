using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.UpdateSession;

public sealed record UpdateSessionCommand(
    Guid TrainingClassId,
    Guid SessionId,
    string Title,
    ClassSessionType SessionType,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Location)
    : IRequest<Result<ClassSessionDto>>;
