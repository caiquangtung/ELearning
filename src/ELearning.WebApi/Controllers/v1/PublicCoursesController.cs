using Asp.Versioning;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/public/courses")]
public sealed class PublicCoursesController(
    ICourseRepository courseRepository,
    IReviewRepository reviewRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? level = null,
        [FromQuery] long? minPriceCents = null,
        [FromQuery] long? maxPriceCents = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 48);
        var parsedSort = Enum.TryParse(sort, true, out CourseSortOption so)
            ? so
            : CourseSortOption.Newest;

        var courses = await courseRepository.ListAsync(
            page: 1,
            pageSize: 200,
            search,
            status: CourseStatus.Published,
            minPriceCents,
            maxPriceCents,
            parsedSort,
            ct);

        var previews = courses.Items.Select(ToPreview).ToList();
        previews = ApplyMetadataFilters(previews, category, level);

        var total = previews.Count;
        var items = previews
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedList<PublicCourseCardDto>.Create(items, page, pageSize, total));
    }

    [HttpGet("featured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Featured([FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var pageSize = Math.Clamp(limit, 1, 12);
        var courses = await courseRepository.ListAsync(
            page: 1,
            pageSize: pageSize,
            search: null,
            status: CourseStatus.Published,
            minPriceCents: null,
            maxPriceCents: null,
            sort: CourseSortOption.Newest,
            ct);

        return Ok(courses.Items.Select(ToPreview));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(id, ct);
        if (course is null || course.Status != CourseStatus.Published)
            return NotFound();

        var category = InferCategory(course.Title, course.Description);
        var level = InferLevel(course.Title, course.Description);
        var summary = await reviewRepository.GetSummaryForCourseAsync(course.Id, ct);
        var reviews = await reviewRepository.ListForCourseAsync(course.Id, page: 1, pageSize: 3, includeRejected: false, ct);
        var sections = course.Sections
            .OrderBy(section => section.SortOrder)
            .Select(section => new PublicCourseSectionDto(
                section.Id,
                section.Title,
                section.SortOrder,
                section.Lessons
                    .OrderBy(lesson => lesson.SortOrder)
                    .Select(lesson => new PublicCourseLessonDto(lesson.Id, lesson.Title, lesson.SortOrder))
                    .ToList()))
            .ToList();

        return Ok(new PublicCourseDetailDto(
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            level,
            category,
            BuildThumbnailUrl(category),
            BuildDurationMinutes(course),
            sections.Sum(section => section.Lessons.Count),
            sections.Count,
            BuildOutcomes(course.Title, category),
            summary.AverageRating,
            summary.ReviewCount,
            sections,
            reviews.Items.Select(review => new PublicCourseReviewDto(
                review.Id,
                review.Rating,
                review.Comment,
                review.SubmittedAt)).ToList()));
    }

    private static List<PublicCourseCardDto> ApplyMetadataFilters(
        List<PublicCourseCardDto> courses,
        string? category,
        string? level)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            courses = courses
                .Where(course => string.Equals(course.Category, category.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            courses = courses
                .Where(course => string.Equals(course.Level, level.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return courses;
    }

    private static string InferLevel(string title, string? description)
    {
        var text = $"{title} {description}".ToLowerInvariant();
        if (text.Contains("advanced") || text.Contains("architecture") || text.Contains("expert"))
            return "Advanced";
        if (text.Contains("fundamentals") || text.Contains("intro") || text.Contains("beginner"))
            return "Beginner";
        return "Intermediate";
    }

    private static string InferCategory(string title, string? description)
    {
        var text = $"{title} {description}".ToLowerInvariant();
        if (HasTerm(text, "ai") || text.Contains("artificial intelligence") || text.Contains("machine learning"))
            return "AI";
        if (text.Contains("security") || text.Contains("secure"))
            return "Security";
        if (text.Contains("devops") || text.Contains("cloud"))
            return "DevOps";
        if (text.Contains("design") || text.Contains("ux"))
            return "Design";
        if (text.Contains("leadership") || text.Contains("sales") || text.Contains("product"))
            return "Business";
        if (text.Contains("data") || text.Contains("analytics"))
            return "Data";
        return "Technology";
    }

    private static bool HasTerm(string text, string term) =>
        Regex.IsMatch(text, $@"(^|[^a-z0-9]){Regex.Escape(term)}([^a-z0-9]|$)", RegexOptions.IgnoreCase);

    private static PublicCourseCardDto ToPreview(Course course)
    {
        var category = InferCategory(course.Title, course.Description);
        var sectionCount = course.Sections.Count;
        var lessonCount = course.Sections.Sum(section => section.Lessons.Count);

        return new PublicCourseCardDto(
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            InferLevel(course.Title, course.Description),
            category,
            BuildThumbnailUrl(category),
            lessonCount == 0 ? 8 : lessonCount,
            sectionCount == 0 ? Math.Max(1, lessonCount / 4) : sectionCount,
            BuildDurationMinutes(course));
    }

    private static int BuildDurationMinutes(Course course)
    {
        var lessonCount = course.Sections.Sum(section => section.Lessons.Count);
        return Math.Max(120, lessonCount * 35);
    }

    private static string BuildThumbnailUrl(string category) =>
        category switch
        {
            "AI" => "/assets/public/course-ai.png",
            "Data" => "/assets/public/course-data.png",
            "Security" => "/assets/public/course-security.png",
            "DevOps" => "/assets/public/course-devops.png",
            "Design" => "/assets/public/course-design.png",
            "Business" => "/assets/public/course-business.png",
            _ => "/assets/public/course-technology.png"
        };

    private static IReadOnlyList<string> BuildOutcomes(string title, string category) =>
        category switch
        {
            "AI" =>
            [
                $"Apply {title} concepts to practical learning workflows.",
                "Use AI tools responsibly with review checkpoints.",
                "Turn course exercises into portfolio-ready practice."
            ],
            "Data" =>
            [
                "Read dashboards and datasets with stronger analytical judgment.",
                "Translate business questions into measurable analysis tasks.",
                "Communicate insights with clear recommendations."
            ],
            "Security" =>
            [
                "Identify common application risk patterns.",
                "Apply secure coding review habits before release.",
                "Connect security controls to real development workflows."
            ],
            "DevOps" =>
            [
                "Understand deployment, observability, and delivery tradeoffs.",
                "Practice repeatable workflows for cloud and release operations.",
                "Improve team handoffs from code to production."
            ],
            "Design" =>
            [
                "Plan research and usability checks around real user tasks.",
                "Convert findings into clearer product decisions.",
                "Improve handoff quality between design and delivery teams."
            ],
            "Business" =>
            [
                "Strengthen team communication and operating rhythm.",
                "Apply structured decision-making in practical scenarios.",
                "Measure outcomes and adapt plans with confidence."
            ],
            _ =>
            [
                "Build a practical foundation through guided lessons.",
                "Practice concepts with structured checkpoints.",
                "Use AI assistance to choose the next learning step."
            ]
        };

    private sealed record PublicCourseCardDto(
        Guid Id,
        string Title,
        string? Description,
        long PriceCents,
        string Currency,
        string Level,
        string Category,
        string ThumbnailUrl,
        int LessonCount,
        int SectionCount,
        int DurationMinutes);

    private sealed record PublicCourseDetailDto(
        Guid Id,
        string Title,
        string? Description,
        long PriceCents,
        string Currency,
        string Level,
        string Category,
        string ThumbnailUrl,
        int DurationMinutes,
        int LessonCount,
        int SectionCount,
        IReadOnlyList<string> Outcomes,
        decimal AverageRating,
        int ReviewCount,
        IReadOnlyList<PublicCourseSectionDto> Sections,
        IReadOnlyList<PublicCourseReviewDto> Reviews);

    private sealed record PublicCourseSectionDto(
        Guid Id,
        string Title,
        int SortOrder,
        IReadOnlyList<PublicCourseLessonDto> Lessons);

    private sealed record PublicCourseLessonDto(Guid Id, string Title, int SortOrder);

    private sealed record PublicCourseReviewDto(Guid Id, int Rating, string Comment, DateTime SubmittedAt);
}
