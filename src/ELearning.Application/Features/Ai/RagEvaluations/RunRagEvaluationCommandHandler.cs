using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Ai.Knowledge;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.RagEvaluations;

public sealed class RunRagEvaluationCommandHandler(
    IAiRagEvaluationService evaluationService,
    ICurrentUserService currentUserService)
    : IRequestHandler<RunRagEvaluationCommand, Result<AiRagEvaluationRunDto>>
{
    public async Task<Result<AiRagEvaluationRunDto>> Handle(
        RunRagEvaluationCommand request,
        CancellationToken ct)
    {
        var result = await evaluationService.RunAsync(
            currentUserService.UserId,
            currentUserService.Roles.ToArray(),
            ct);

        return RagEvaluationMapper.ToDto(result);
    }
}
