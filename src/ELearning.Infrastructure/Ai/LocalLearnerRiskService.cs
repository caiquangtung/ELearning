using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Domain.Aggregates.VideoAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed class LocalLearnerRiskService(ApplicationDbContext context) : IAiLearnerRiskService
{
    public async Task<AiLearnerRiskResult> GetLearnerRiskAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var videoSignals = await context.Set<WatchEvent>()
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => new
            {
                w.ProgressPercent,
                ActivityAt = w.UpdatedAt ?? w.CreatedAt
            })
            .ToListAsync(ct);

        var quizSignals = await context.Set<QuizAttempt>()
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new
            {
                a.Status,
                a.TotalScore,
                ActivityAt = a.SubmittedAt ?? a.UpdatedAt ?? a.CreatedAt
            })
            .ToListAsync(ct);

        var licenseSignals = await (
            from assignment in context.Set<LicenseAssignment>().AsNoTracking()
            join pool in context.Set<LicensePool>().AsNoTracking()
                on assignment.LicensePoolId equals pool.Id
            where assignment.UserId == userId && assignment.RevokedAt == null
            select pool.ExpiresAt)
            .ToListAsync(ct);

        var averageVideoProgress = videoSignals.Count == 0
            ? (decimal?)null
            : Math.Round(videoSignals.Average(x => x.ProgressPercent), 2);

        var scoredQuizzes = quizSignals
            .Where(x => x.Status == AttemptStatus.Graded && x.TotalScore.HasValue)
            .Select(x => x.TotalScore!.Value)
            .ToList();

        var averageQuizScore = scoredQuizzes.Count == 0
            ? (decimal?)null
            : Math.Round((decimal)scoredQuizzes.Average(), 2);

        var lastActivityAt = videoSignals.Select(x => (DateTime?)x.ActivityAt)
            .Concat(quizSignals.Select(x => (DateTime?)x.ActivityAt))
            .Where(x => x.HasValue)
            .Max();

        var daysSinceLastActivity = lastActivityAt.HasValue
            ? Math.Max(0, (int)Math.Floor((now - lastActivityAt.Value).TotalDays))
            : (int?)null;

        var nearestLicenseExpiry = licenseSignals
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty()
            .Min();
        DateTime? nearestExpiry = nearestLicenseExpiry == default ? null : nearestLicenseExpiry;

        var signals = new AiLearnerRiskSignals(
            averageVideoProgress,
            averageQuizScore,
            lastActivityAt,
            daysSinceLastActivity,
            licenseSignals.Count,
            nearestExpiry);

        return Score(userId, signals, now);
    }

    public async Task<AiOrganizationRiskReportResult> GetOrganizationRiskReportAsync(
        Guid organizationId,
        CancellationToken ct = default)
    {
        var learnerIds = await context.Set<OrganizationMember>()
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        var learners = new List<AiLearnerRiskResult>();
        foreach (var learnerId in learnerIds)
        {
            learners.Add(await GetLearnerRiskAsync(learnerId, ct));
        }

        var ordered = learners
            .OrderByDescending(x => x.RiskScore)
            .ThenBy(x => x.UserId)
            .ToList();

        return new AiOrganizationRiskReportResult(
            organizationId,
            ordered,
            ordered.Count(x => x.RiskLevel == "High"),
            ordered.Count(x => x.RiskLevel == "Medium"),
            ordered.Count(x => x.RiskLevel == "Low"));
    }

    private static AiLearnerRiskResult Score(Guid userId, AiLearnerRiskSignals signals, DateTime now)
    {
        var risk = 10;
        var reasons = new List<string>();
        var actions = new List<string>();

        if (signals.AverageVideoProgress.HasValue)
        {
            if (signals.AverageVideoProgress < 25)
            {
                risk += 35;
                reasons.Add("Average video progress is below 25%.");
                actions.Add("Send a reminder with the next lesson link.");
            }
            else if (signals.AverageVideoProgress < 60)
            {
                risk += 18;
                reasons.Add("Video progress is behind the expected pace.");
                actions.Add("Recommend a short catch-up session.");
            }
        }
        else
        {
            risk += 8;
            reasons.Add("No video progress is available yet.");
            actions.Add("Wait for initial activity before escalating.");
        }

        if (signals.AverageQuizScore.HasValue)
        {
            if (signals.AverageQuizScore < 50)
            {
                risk += 30;
                reasons.Add("Average quiz score is below 50.");
                actions.Add("Assign remedial material before the next assessment.");
            }
            else if (signals.AverageQuizScore < 70)
            {
                risk += 16;
                reasons.Add("Quiz performance is below the pass-ready range.");
                actions.Add("Review weak topics with the learner.");
            }
        }

        if (signals.DaysSinceLastActivity.HasValue)
        {
            if (signals.DaysSinceLastActivity >= 14)
            {
                risk += 28;
                reasons.Add("No learning activity in the last 14 days.");
                actions.Add("Trigger instructor or organization admin follow-up.");
            }
            else if (signals.DaysSinceLastActivity >= 7)
            {
                risk += 14;
                reasons.Add("No learning activity in the last 7 days.");
                actions.Add("Send a nudge notification.");
            }
        }
        else
        {
            risk += 10;
            reasons.Add("No recent learning activity is recorded.");
        }

        if (signals.NearestLicenseExpiry.HasValue)
        {
            var daysToExpiry = (int)Math.Ceiling((signals.NearestLicenseExpiry.Value - now).TotalDays);
            if (daysToExpiry <= 7)
            {
                risk += 18;
                reasons.Add("Assigned license expires within 7 days.");
                actions.Add("Prioritize remaining required lessons before expiry.");
            }
            else if (daysToExpiry <= 21)
            {
                risk += 8;
                reasons.Add("Assigned license expires within 21 days.");
            }
        }

        risk = Math.Clamp(risk, 0, 100);
        var level = risk >= 70 ? "High" : risk >= 40 ? "Medium" : "Low";

        if (actions.Count == 0)
        {
            actions.Add("Keep monitoring normal progress.");
        }
        if (reasons.Count == 0)
        {
            reasons.Add("Learner activity is currently within normal range.");
        }

        return new AiLearnerRiskResult(userId, risk, level, reasons.Distinct().ToList(), actions.Distinct().ToList(), signals);
    }
}
