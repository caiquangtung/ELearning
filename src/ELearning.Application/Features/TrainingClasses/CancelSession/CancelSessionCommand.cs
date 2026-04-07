using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.CancelSession;

public sealed record CancelSessionCommand(Guid TrainingClassId, Guid SessionId) : IRequest<Result>;
