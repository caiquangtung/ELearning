using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.LearnerRisk;

public sealed record GetLearnerRiskQuery(Guid UserId) : IRequest<Result<LearnerRiskDto>>;
