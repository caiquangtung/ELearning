namespace ELearning.Application.Common.Interfaces;

public interface IAiLearnerRiskService
{
    Task<AiLearnerRiskResult> GetLearnerRiskAsync(Guid userId, CancellationToken ct = default);
    Task<AiOrganizationRiskReportResult> GetOrganizationRiskReportAsync(Guid organizationId, CancellationToken ct = default);
}

public sealed record AiLearnerRiskResult(
    Guid UserId,
    int RiskScore,
    string RiskLevel,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> RecommendedActions,
    AiLearnerRiskSignals Signals);

public sealed record AiOrganizationRiskReportResult(
    Guid OrganizationId,
    IReadOnlyList<AiLearnerRiskResult> Learners,
    int HighRiskCount,
    int MediumRiskCount,
    int LowRiskCount);

public sealed record AiLearnerRiskSignals(
    decimal? AverageVideoProgress,
    decimal? AverageQuizScore,
    DateTime? LastActivityAt,
    int? DaysSinceLastActivity,
    int ActiveLicenseCount,
    DateTime? NearestLicenseExpiry);
