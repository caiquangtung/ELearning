using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.LearnerRisk;

public sealed class GetLearnerRiskQueryHandler(
    IAiLearnerRiskService riskService,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetLearnerRiskQuery, Result<LearnerRiskDto>>
{
    private const string Provider = "Local";
    private const string Model = "local-risk-predictor-v1";
    private const string PromptVersion = "learner-risk-v1";

    public async Task<Result<LearnerRiskDto>> Handle(GetLearnerRiskQuery request, CancellationToken ct)
    {
        var inputHash = ComputeInputHash(request.UserId);

        try
        {
            var risk = await riskService.GetLearnerRiskAsync(request.UserId, ct);
            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "LearnerRiskPrediction",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                tokenEstimate: null));
            await unitOfWork.SaveChangesAsync(ct);
            return LearnerRiskMapper.ToDto(risk);
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "LearnerRiskPrediction",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<LearnerRiskDto>(Error.Validation("AI.Risk", ex.Message));
        }
    }

    private static string ComputeInputHash(Guid userId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}|{PromptVersion}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
