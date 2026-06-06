using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OpenAiCompatibleLearningPathService(
    ApplicationDbContext context,
    OpenAiCompatibleChatClient client,
    IOptions<AiOptions> options)
    : IAiLearningPathService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public string CacheVariant
    {
        get
        {
            var config = options.Value;
            var model = config.ResolveChatModel();
            if (string.IsNullOrWhiteSpace(model))
                model = "unconfigured-model";
            var promptVersion = string.IsNullOrWhiteSpace(config.LearningPathPromptVersion)
                ? "learning-path-generator-v1"
                : config.LearningPathPromptVersion;

            return $"openai-compatible:{model}:{promptVersion}";
        }
    }

    public async Task<AiLearningPathDraft> GenerateAsync(AiLearningPathRequest request, CancellationToken ct = default)
    {
        var config = options.Value;
        var maxCourses = Math.Clamp(request.MaxCourses, 1, 12);
        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .OrderByDescending(c => c.CreatedAt)
            .Take(40)
            .ToListAsync(ct);

        if (courses.Count == 0)
            throw new InvalidOperationException("No published courses are available for learning path generation.");

        var result = await client.CompleteJsonAsync(
            BuildSystemPrompt(),
            BuildUserPrompt(request, maxCourses, courses),
            ct);

        var response = DeserializeResponse(result.Content);

        if (response is null)
            throw new InvalidOperationException("AI provider returned an invalid learning path payload.");

        if (response.Confidence < 0 || response.Confidence > 1)
            throw new InvalidOperationException("AI provider returned confidence outside the 0-1 range.");

        var courseMap = courses.ToDictionary(c => c.Id);
        var pathCourses = BuildPathCourses(response, courseMap, maxCourses);
        if (pathCourses.Count == 0)
            throw new InvalidOperationException("AI provider did not return any known course IDs for the learning path.");

        return new AiLearningPathDraft(
            result.Provider,
            result.Model,
            string.IsNullOrWhiteSpace(config.LearningPathPromptVersion)
                ? "learning-path-generator-v1"
                : config.LearningPathPromptVersion,
            result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(
                request.Goal,
                request.CurrentSkills,
                request.TargetRole,
                result.Content),
            request.Goal,
            request.TargetRole,
            Math.Round(response.Confidence, 2),
            string.IsNullOrWhiteSpace(response.EstimatedEffort)
                ? EstimateTotalEffort(pathCourses.Count)
                : response.EstimatedEffort.Trim(),
            NormalizeMissingSkills(response.MissingSkills),
            pathCourses);
    }

    internal static List<AiLearningPathCourse> BuildPathCoursesFromProviderJson(
        string providerContent,
        IReadOnlyList<Course> courses,
        int maxCourses)
    {
        var response = DeserializeResponse(providerContent)
            ?? throw new InvalidOperationException("AI provider returned an invalid learning path payload.");
        var courseMap = courses.ToDictionary(c => c.Id);
        return BuildPathCourses(response, courseMap, Math.Clamp(maxCourses, 1, 12));
    }

    private static LearningPathResponse? DeserializeResponse(string content) =>
        JsonSerializer.Deserialize<LearningPathResponse>(
            OpenAiCompatibleJson.ExtractObject(content),
            JsonOptions);

    private static List<AiLearningPathCourse> BuildPathCourses(
        LearningPathResponse response,
        IReadOnlyDictionary<Guid, Course> courseMap,
        int maxCourses)
    {
        var seen = new HashSet<Guid>();
        var pathCourses = new List<AiLearningPathCourse>();

        foreach (var item in response.Courses ?? [])
        {
            if (!Guid.TryParse(item.CourseId, out var courseId) ||
                !courseMap.TryGetValue(courseId, out var course) ||
                !seen.Add(courseId))
            {
                continue;
            }

            var reasons = (item.Reasons ?? [])
                .Where(reason => !string.IsNullOrWhiteSpace(reason))
                .Select(reason => reason.Trim())
                .Take(4)
                .ToList();

            if (reasons.Count == 0)
                reasons.Add("Selected by the AI provider for the stated learning goal.");

            pathCourses.Add(new AiLearningPathCourse(
                pathCourses.Count + 1,
                course.Id,
                course.Title,
                course.Description,
                course.PriceCents,
                course.Currency,
                Math.Round(Math.Clamp(item.Score, 0m, 100m), 2),
                string.IsNullOrWhiteSpace(item.EstimatedEffort)
                    ? EstimateEffort(pathCourses.Count)
                    : item.EstimatedEffort.Trim(),
                reasons));

            if (pathCourses.Count == maxCourses)
                break;
        }

        return pathCourses;
    }

    private static IReadOnlyList<string> NormalizeMissingSkills(IReadOnlyList<string>? values)
    {
        var normalized = (values ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToList();

        return normalized.Count == 0
            ? ["No major missing skill detected from the current catalog."]
            : normalized;
    }

    private static string BuildSystemPrompt() =>
        """
        You create draft LMS learning paths from a learner goal and a provided course catalog. Return only a JSON object.
        Use only courseId values from the provided catalog. Do not invent course IDs.
        The JSON shape must be:
        {"confidence":0.0,"estimatedEffort":"1-6 weeks","missingSkills":["..."],"courses":[{"courseId":"guid","score":0,"estimatedEffort":"1-2 weeks","reasons":["..."]}]}
        confidence must be between 0 and 1. course score must be between 0 and 100.
        """;

    private static string BuildUserPrompt(AiLearningPathRequest request, int maxCourses, IReadOnlyList<Course> courses)
    {
        var payload = new
        {
            request.Goal,
            request.CurrentSkills,
            request.TargetRole,
            request.OrganizationId,
            MaxCourses = maxCourses,
            Catalog = courses.Select(course => new
            {
                CourseId = course.Id,
                course.Title,
                course.Description,
                course.PriceCents,
                course.Currency,
                Lessons = course.Sections
                    .SelectMany(section => section.Lessons)
                    .Select(lesson => new { lesson.Title, lesson.Content })
                    .Take(8)
                    .ToArray()
            }).ToArray()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string EstimateEffort(int index) => index switch
    {
        0 => "1-2 weeks",
        1 or 2 => "2-3 weeks",
        _ => "3-4 weeks"
    };

    private static string EstimateTotalEffort(int courseCount)
    {
        var low = Math.Max(1, courseCount);
        var high = Math.Max(low + 1, courseCount * 3);
        return $"{low}-{high} weeks";
    }

    private sealed record LearningPathResponse(
        decimal Confidence,
        string? EstimatedEffort,
        IReadOnlyList<string>? MissingSkills,
        IReadOnlyList<LearningPathCourseItem>? Courses);

    private sealed record LearningPathCourseItem(
        string? CourseId,
        decimal Score,
        string? EstimatedEffort,
        IReadOnlyList<string>? Reasons);
}
