using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed class LocalSemanticSearchService(
    ApplicationDbContext context,
    IAiEmbeddingService embeddingService)
    : IAiSemanticSearchService
{
    public async Task<IReadOnlyList<AiSemanticCourseSearchResult>> SearchCoursesAsync(
        string query,
        int limit,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 20);
        var queryEmbedding = embeddingService.Embed(query);

        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .ToListAsync(ct);

        return courses
            .Select(course => ScoreCourse(course, query, queryEmbedding))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
    }

    private AiSemanticCourseSearchResult ScoreCourse(
        Course course,
        string query,
        IReadOnlyDictionary<string, decimal> queryEmbedding)
    {
        var courseText = BuildCourseText(course);
        var courseEmbedding = embeddingService.Embed(courseText);
        var similarity = embeddingService.CosineSimilarity(queryEmbedding, courseEmbedding);
        var matchedConcepts = embeddingService.TopSharedTerms(queryEmbedding, courseEmbedding, 6);
        var keywordFallback = KeywordFallbackScore(query, courseText);
        var score = Math.Round(Math.Max(similarity * 100m, keywordFallback), 2);

        var reasons = new List<string>();
        if (matchedConcepts.Count > 0)
            reasons.Add($"Matches concepts: {string.Join(", ", matchedConcepts.Take(4))}.");
        if (similarity >= 0.15m)
            reasons.Add("Ranked by local embedding vector similarity.");
        if (keywordFallback > similarity * 100m)
            reasons.Add("Keyword fallback boosted this match.");
        if (reasons.Count == 0 && score > 0)
            reasons.Add("Related catalog content matched the query.");

        return new AiSemanticCourseSearchResult(
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            course.CreatedAt,
            score,
            matchedConcepts,
            reasons);
    }

    private static string BuildCourseText(Course course)
    {
        var lessonText = course.Sections
            .SelectMany(s => s.Lessons)
            .Select(l => $"{l.Title} {l.Content}");
        return string.Join(' ', new[] { course.Title, course.Description ?? "" }.Concat(lessonText));
    }

    private static decimal KeywordFallbackScore(string query, string courseText)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 3)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .ToList();
        if (terms.Count == 0)
            return 0m;

        var normalizedCourse = courseText.ToLowerInvariant();
        var matches = terms.Count(normalizedCourse.Contains);
        return Math.Round(matches * 65m / terms.Count, 2);
    }
}
