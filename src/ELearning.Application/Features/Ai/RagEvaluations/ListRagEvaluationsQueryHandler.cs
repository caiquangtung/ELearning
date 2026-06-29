using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Ai.Knowledge;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.RagEvaluations;

public sealed class ListRagEvaluationsQueryHandler(IAiRagEvaluationService evaluationService)
    : IRequestHandler<ListRagEvaluationsQuery, Result<IReadOnlyList<AiRagEvaluationRunDto>>>
{
    public async Task<Result<IReadOnlyList<AiRagEvaluationRunDto>>> Handle(
        ListRagEvaluationsQuery request,
        CancellationToken ct)
    {
        var runs = await evaluationService.ListAsync(ct);
        return runs.Select(RagEvaluationMapper.ToDto).ToList();
    }
}
