using Asp.Versioning;
using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CourseAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/public/courses")]
public sealed class PublicCoursesController(ICourseRepository courseRepository) : ControllerBase
{
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

        return Ok(courses.Items.Select(course => new PublicFeaturedCourseDto(
            course.Id,
            course.Title,
            course.Description,
            course.PriceCents,
            course.Currency,
            InferLevel(course.Title, course.Description),
            InferCategory(course.Title, course.Description))));
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
        if (text.Contains("data") || text.Contains("analytics") || text.Contains("ai"))
            return "Data";
        if (text.Contains("security") || text.Contains("secure"))
            return "Security";
        if (text.Contains("devops") || text.Contains("cloud"))
            return "DevOps";
        if (text.Contains("design") || text.Contains("ux"))
            return "Design";
        if (text.Contains("leadership") || text.Contains("sales"))
            return "Business";
        return "Technology";
    }

    private sealed record PublicFeaturedCourseDto(
        Guid Id,
        string Title,
        string? Description,
        long PriceCents,
        string Currency,
        string Level,
        string Category);
}
