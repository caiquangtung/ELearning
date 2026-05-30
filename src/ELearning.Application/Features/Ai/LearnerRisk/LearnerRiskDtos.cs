namespace ELearning.Application.Features.Ai.LearnerRisk;

public sealed record LearnerRiskDto(
    Guid UserId,
    int RiskScore,
    string RiskLevel,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> RecommendedActions,
    LearnerRiskSignalsDto Signals);

public sealed record LearnerRiskSignalsDto(
    decimal? AverageVideoProgress,
    decimal? AverageQuizScore,
    DateTime? LastActivityAt,
    int? DaysSinceLastActivity,
    int ActiveLicenseCount,
    DateTime? NearestLicenseExpiry);

public sealed record OrganizationRiskReportDto(
    Guid OrganizationId,
    int LearnerCount,
    int HighRiskCount,
    int MediumRiskCount,
    int LowRiskCount,
    IReadOnlyList<LearnerRiskDto> Learners);
