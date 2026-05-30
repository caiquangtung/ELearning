using ELearning.Application.Common.Interfaces;

namespace ELearning.Application.Features.Ai.LearnerRisk;

internal static class LearnerRiskMapper
{
    public static LearnerRiskDto ToDto(AiLearnerRiskResult risk) => new(
        risk.UserId,
        risk.RiskScore,
        risk.RiskLevel,
        risk.Reasons,
        risk.RecommendedActions,
        new LearnerRiskSignalsDto(
            risk.Signals.AverageVideoProgress,
            risk.Signals.AverageQuizScore,
            risk.Signals.LastActivityAt,
            risk.Signals.DaysSinceLastActivity,
            risk.Signals.ActiveLicenseCount,
            risk.Signals.NearestLicenseExpiry));
}
