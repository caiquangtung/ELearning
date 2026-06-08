using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed partial class LocalCourseRecommendationService(ApplicationDbContext context)
    : IAiCourseRecommendationService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "into", "this", "that", "your", "you",
        "course", "lesson", "fundamentals", "introduction", "basic", "advanced",
        "hoc", "khoa", "bai", "can", "ban", "nang", "cao"
    };

    public async Task<IReadOnlyList<AiCourseRecommendationCandidate>> RecommendAsync(
        Guid userId,
        int limit,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 20);

        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .ToListAsync(ct);

        if (courses.Count == 0)
            return [];

        var memberships = await context.Set<OrganizationMember>()
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.OrganizationId)
            .ToListAsync(ct);

        var paidCourseItems = await context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Paid)
            .SelectMany(o => o.Items
                .Where(i => i.ItemType == OrderItemType.Course)
                .Select(i => new PaidCourseSignal(i.ReferenceId, o.BuyerUserId, o.OrganizationId)))
            .ToListAsync(ct);

        var purchasedCourseIds = paidCourseItems
            .Where(i => i.BuyerUserId == userId)
            .Select(i => i.CourseId)
            .ToHashSet();

        var popularityByCourse = paidCourseItems
            .GroupBy(i => i.CourseId)
            .ToDictionary(g => g.Key, g => g.Select(i => i.BuyerUserId).Distinct().Count());

        var organizationPopularityByCourse = paidCourseItems
            .Where(i => i.OrganizationId.HasValue && memberships.Contains(i.OrganizationId.Value))
            .GroupBy(i => i.CourseId)
            .ToDictionary(g => g.Key, g => g.Select(i => i.BuyerUserId).Distinct().Count());

        var quizSignals = await (
            from attempt in context.QuizAttempts.AsNoTracking()
            join quiz in context.Quizzes.AsNoTracking() on attempt.QuizId equals quiz.Id
            where attempt.UserId == userId && quiz.CourseId.HasValue
            select new
            {
                CourseId = quiz.CourseId!.Value,
                attempt.TotalScore,
                attempt.Status
            })
            .ToListAsync(ct);

        var completedQuizCourseIds = quizSignals
            .Where(x => x.Status == AttemptStatus.Graded && x.TotalScore >= 70)
            .Select(x => x.CourseId)
            .ToHashSet();

        var averageQuizScore = quizSignals
            .Where(x => x.TotalScore.HasValue)
            .Select(x => x.TotalScore!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var watchSignals = await (
            from watch in context.WatchEvents.AsNoTracking()
            join lesson in context.Set<Lesson>().AsNoTracking() on watch.LessonId equals lesson.Id
            join section in context.Set<Section>().AsNoTracking() on lesson.SectionId equals section.Id
            where watch.UserId == userId
            select new
            {
                section.CourseId,
                watch.ProgressPercent,
                watch.IsCompleted
            })
            .ToListAsync(ct);

        var watchedCourseIds = watchSignals
            .Where(x => x.ProgressPercent > 0)
            .Select(x => x.CourseId)
            .ToHashSet();

        var completedWatchCourseIds = watchSignals
            .Where(x => x.IsCompleted)
            .Select(x => x.CourseId)
            .ToHashSet();

        var historyCourseIds = purchasedCourseIds
            .Concat(completedQuizCourseIds)
            .Concat(watchedCourseIds)
            .Concat(completedWatchCourseIds)
            .Distinct()
            .ToHashSet();

        var historyTerms = courses
            .Where(c => historyCourseIds.Contains(c.Id))
            .SelectMany(TokenizeCourse)
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var maxPopularity = Math.Max(1, popularityByCourse.Values.DefaultIfEmpty(0).Max());
        var maxOrganizationPopularity = Math.Max(1, organizationPopularityByCourse.Values.DefaultIfEmpty(0).Max());
        var now = DateTime.UtcNow;

        var ranked = courses
            .Select(course => ScoreCourse(
                course,
                purchasedCourseIds,
                completedQuizCourseIds,
                watchedCourseIds,
                completedWatchCourseIds,
                historyTerms,
                popularityByCourse,
                organizationPopularityByCourse,
                maxPopularity,
                maxOrganizationPopularity,
                averageQuizScore,
                now))
            .Where(x => !purchasedCourseIds.Contains(x.CourseId))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();

        if (ranked.Count >= limit || purchasedCourseIds.Count == 0)
            return ranked;

        var fallback = courses
            .Where(c => !ranked.Any(r => r.CourseId == c.Id))
            .Select(course => ScoreCourse(
                course,
                purchasedCourseIds,
                completedQuizCourseIds,
                watchedCourseIds,
                completedWatchCourseIds,
                historyTerms,
                popularityByCourse,
                organizationPopularityByCourse,
                maxPopularity,
                maxOrganizationPopularity,
                averageQuizScore,
                now,
                forceFallback: true))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit - ranked.Count);

        ranked.AddRange(fallback);
        return ranked;
    }

    private static AiCourseRecommendationCandidate ScoreCourse(
        Course course,
        IReadOnlySet<Guid> purchasedCourseIds,
        IReadOnlySet<Guid> completedQuizCourseIds,
        IReadOnlySet<Guid> watchedCourseIds,
        IReadOnlySet<Guid> completedWatchCourseIds,
        IReadOnlyDictionary<string, int> historyTerms,
        IReadOnlyDictionary<Guid, int> popularityByCourse,
        IReadOnlyDictionary<Guid, int> organizationPopularityByCourse,
        int maxPopularity,
        int maxOrganizationPopularity,
        double averageQuizScore,
        DateTime now,
        bool forceFallback = false)
    {
        var reasons = new List<string>();
        var signals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var popularity = popularityByCourse.GetValueOrDefault(course.Id);
        var popularitySignal = Math.Round(popularity * 25m / maxPopularity, 2);
        signals["popularity"] = popularitySignal;
        if (popularity > 0)
            reasons.Add("Popular with other learners.");

        var organizationPopularity = organizationPopularityByCourse.GetValueOrDefault(course.Id);
        var organizationSignal = Math.Round(organizationPopularity * 15m / maxOrganizationPopularity, 2);
        signals["organizationPopularity"] = organizationSignal;
        if (organizationPopularity > 0)
            reasons.Add("Popular in your organization.");

        var similaritySignal = CalculateSimilaritySignal(course, historyTerms);
        signals["contentSimilarity"] = similaritySignal;
        if (similaritySignal >= 10)
            reasons.Add("Matches topics from courses you viewed or completed.");

        var learnerProgressSignal = 0m;
        if (watchedCourseIds.Contains(course.Id))
            learnerProgressSignal += 4m;
        if (completedWatchCourseIds.Contains(course.Id))
            learnerProgressSignal += 3m;
        if (completedQuizCourseIds.Contains(course.Id))
            learnerProgressSignal += 3m;
        signals["learnerProgress"] = learnerProgressSignal;
        if (learnerProgressSignal > 0)
            reasons.Add("Continues your current learning activity.");

        var quizPerformanceSignal = averageQuizScore >= 80
            ? 5m
            : averageQuizScore >= 60
                ? 3m
                : 0m;
        signals["quizPerformance"] = quizPerformanceSignal;
        if (quizPerformanceSignal > 0)
            reasons.Add("Fits your recent quiz performance.");

        var ageDays = Math.Max(0, (now - course.CreatedAt).TotalDays);
        var freshnessSignal = Math.Round(Math.Max(0m, 5m - (decimal)ageDays / 30m), 2);
        signals["freshness"] = freshnessSignal;
        if (freshnessSignal >= 4)
            reasons.Add("Recently added to the catalog.");

        var alreadyPurchasedPenalty = purchasedCourseIds.Contains(course.Id) ? -50m : 0m;
        signals["alreadyPurchasedPenalty"] = alreadyPurchasedPenalty;

        var hasHistory = historyTerms.Count > 0;
        var isFallback = forceFallback || !hasHistory;
        if (isFallback && reasons.Count == 0)
            reasons.Add("Recommended as a strong catalog fallback.");

        var score = 10m
            + popularitySignal
            + organizationSignal
            + similaritySignal
            + learnerProgressSignal
            + quizPerformanceSignal
            + freshnessSignal
            + alreadyPurchasedPenalty;

        if (isFallback)
            score = Math.Max(score, 20m + popularitySignal + freshnessSignal);

        return new AiCourseRecommendationCandidate(
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            course.CreatedAt,
            Math.Round(Math.Max(0m, score), 2),
            isFallback,
            reasons.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList(),
            signals);
    }

    private static decimal CalculateSimilaritySignal(Course course, IReadOnlyDictionary<string, int> historyTerms)
    {
        if (historyTerms.Count == 0)
            return 0m;

        var courseTerms = TokenizeCourse(course).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (courseTerms.Count == 0)
            return 0m;

        var weightedMatches = courseTerms.Sum(term => historyTerms.GetValueOrDefault(term));
        var normalized = Math.Min(1m, weightedMatches / (decimal)Math.Max(1, historyTerms.Values.Sum()));
        return Math.Round(normalized * 35m, 2);
    }

    private static IEnumerable<string> TokenizeCourse(Course course)
    {
        foreach (var token in Tokenize(course.Title))
            yield return token;

        foreach (var token in Tokenize(course.Description))
            yield return token;

        foreach (var lesson in course.Sections.SelectMany(s => s.Lessons))
        {
            foreach (var token in Tokenize(lesson.Title))
                yield return token;
        }
    }

    private static IEnumerable<string> Tokenize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            yield break;

        foreach (Match match in WordRegex().Matches(value.ToLowerInvariant()))
        {
            var token = match.Value;
            if (token.Length < 3 || StopWords.Contains(token))
                continue;

            yield return token;
        }
    }

    private sealed record PaidCourseSignal(Guid CourseId, Guid BuyerUserId, Guid? OrganizationId);

    [GeneratedRegex("[\\p{L}\\p{N}]+", RegexOptions.Compiled)]
    private static partial Regex WordRegex();
}
