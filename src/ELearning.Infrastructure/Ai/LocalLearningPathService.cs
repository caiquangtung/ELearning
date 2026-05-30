using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed class LocalLearningPathService(
    ApplicationDbContext context,
    IAiEmbeddingService embeddingService)
    : IAiLearningPathService
{
    public async Task<AiLearningPathDraft> GenerateAsync(AiLearningPathRequest request, CancellationToken ct = default)
    {
        var maxCourses = Math.Clamp(request.MaxCourses, 1, 12);
        var intentText = string.Join(' ', request.Goal, request.CurrentSkills, request.TargetRole);
        var intentEmbedding = embeddingService.Embed(intentText);

        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .ToListAsync(ct);

        var ranked = courses
            .Select(course => ScoreCourse(course, intentEmbedding, request))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.PriceCents)
            .Take(maxCourses)
            .Select((course, index) => course with
            {
                Order = index + 1,
                EstimatedEffort = EstimateEffort(index)
            })
            .ToList();

        var missingSkills = InferMissingSkills(request, ranked);
        var confidence = ranked.Count == 0
            ? 0m
            : Math.Round(Math.Min(0.95m, ranked.Average(x => x.Score) / 100m), 2);

        return new AiLearningPathDraft(
            request.Goal,
            request.TargetRole,
            confidence,
            EstimateTotalEffort(ranked.Count),
            missingSkills,
            ranked);
    }

    private AiLearningPathCourse ScoreCourse(
        Course course,
        IReadOnlyDictionary<string, decimal> intentEmbedding,
        AiLearningPathRequest request)
    {
        var courseText = BuildCourseText(course);
        var courseEmbedding = embeddingService.Embed(courseText);
        var similarity = embeddingService.CosineSimilarity(intentEmbedding, courseEmbedding);
        var sharedTerms = embeddingService.TopSharedTerms(intentEmbedding, courseEmbedding, 5);
        var score = Math.Round(similarity * 100m, 2);

        var reasons = new List<string>();
        if (sharedTerms.Count > 0)
            reasons.Add($"Covers relevant concepts: {string.Join(", ", sharedTerms.Take(4))}.");
        if (!string.IsNullOrWhiteSpace(request.TargetRole))
            reasons.Add($"Supports the target role: {request.TargetRole}.");
        if (course.PriceCents == 0)
            reasons.Add("Good early path item because it is free.");
        if (reasons.Count == 0 && score > 0)
            reasons.Add("Useful catalog item for the stated learning goal.");

        return new AiLearningPathCourse(
            0,
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            score,
            "",
            reasons);
    }

    private static string BuildCourseText(Course course)
    {
        var lessonText = course.Sections
            .SelectMany(s => s.Lessons)
            .Select(l => $"{l.Title} {l.Content}");
        return string.Join(' ', new[] { course.Title, course.Description ?? "" }.Concat(lessonText));
    }

    private static string EstimateEffort(int index) => index switch
    {
        0 => "1-2 weeks",
        1 or 2 => "2-3 weeks",
        _ => "3-4 weeks"
    };

    private static string EstimateTotalEffort(int courseCount)
    {
        if (courseCount == 0) return "No matching courses yet";
        var low = Math.Max(1, courseCount * 1);
        var high = Math.Max(low + 1, courseCount * 3);
        return $"{low}-{high} weeks";
    }

    private static IReadOnlyList<string> InferMissingSkills(AiLearningPathRequest request, IReadOnlyList<AiLearningPathCourse> ranked)
    {
        var goal = $"{request.Goal} {request.TargetRole}".ToLowerInvariant();
        var covered = string.Join(' ', ranked.SelectMany(x => x.Reasons)).ToLowerInvariant();
        var suggestions = new List<string>();

        foreach (var skill in new[] { "backend", "frontend", "database", "security", "cloud", "testing", "devops", "analytics", "leadership" })
        {
            if (goal.Contains(skill, StringComparison.OrdinalIgnoreCase) &&
                !covered.Contains(skill, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add($"Add more {skill} practice if available.");
            }
        }

        if (suggestions.Count == 0)
            suggestions.Add("No major missing skill detected from the current catalog.");

        return suggestions.Take(4).ToList();
    }
}
