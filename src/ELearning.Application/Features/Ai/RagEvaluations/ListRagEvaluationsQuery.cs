using ELearning.Application.Features.Ai.Knowledge;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.RagEvaluations;

public sealed record ListRagEvaluationsQuery : IRequest<Result<IReadOnlyList<AiRagEvaluationRunDto>>>;
