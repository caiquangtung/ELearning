using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.LearnerRisk;

public sealed class GetOrganizationRiskReportQueryHandler(
    IAiLearnerRiskService riskService,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrganizationRiskReportQuery, Result<OrganizationRiskReportDto>>
{
    private const string Provider = "Local";
    private const string Model = "local-risk-predictor-v1";
    private const string PromptVersion = "organization-risk-report-v1";

    public async Task<Result<OrganizationRiskReportDto>> Handle(GetOrganizationRiskReportQuery request, CancellationToken ct)
    {
        var inputHash = ComputeInputHash(request.OrganizationId);

        try
        {
            var report = await riskService.GetOrganizationRiskReportAsync(request.OrganizationId, ct);
            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "OrganizationRiskReport",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                tokenEstimate: null));
            await unitOfWork.SaveChangesAsync(ct);

            return new OrganizationRiskReportDto(
                report.OrganizationId,
                report.Learners.Count,
                report.HighRiskCount,
                report.MediumRiskCount,
                report.LowRiskCount,
                report.Learners.Select(LearnerRiskMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "OrganizationRiskReport",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<OrganizationRiskReportDto>(Error.Validation("AI.RiskReport", ex.Message));
        }
    }

    private static string ComputeInputHash(Guid organizationId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{organizationId}|{PromptVersion}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
