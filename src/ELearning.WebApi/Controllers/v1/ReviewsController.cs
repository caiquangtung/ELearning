using Asp.Versioning;
using ELearning.Application.Features.Reviews.GetCourseRatingSummary;
using ELearning.Application.Features.Reviews.ListCourseReviews;
using ELearning.Application.Features.Reviews.ModerateReview;
using ELearning.Application.Features.Reviews.SubmitReview;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.WebApi.Authorization;
using ELearning.WebApi.Contracts.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Authorize]
public sealed class ReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet("api/v{version:apiVersion}/courses/{courseId:guid}/reviews")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListForCourse(
        Guid courseId,
        [FromQuery] ListCourseReviewsRequest query,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ListCourseReviewsQuery(
            courseId,
            query.Page,
            query.PageSize,
            query.IncludeRejected), ct);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("api/v{version:apiVersion}/courses/{courseId:guid}/reviews/summary")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(Guid courseId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCourseRatingSummaryQuery(courseId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("api/v{version:apiVersion}/courses/{courseId:guid}/reviews")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(Guid courseId, [FromBody] SubmitReviewRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new SubmitReviewCommand(courseId, body.Rating, body.Comment), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("api/v{version:apiVersion}/reviews/{id:guid}/moderate")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Moderate(Guid id, [FromBody] ModerateReviewRequest body, CancellationToken ct)
    {
        if (!Enum.TryParse<ReviewStatus>(body.Status, true, out var status))
            return Problem(Error.Validation("ReviewStatus", "Review status is invalid."));

        var result = await mediator.Send(new ModerateReviewCommand(id, status, body.Reason), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var c when c.Contains("Conflict") => StatusCodes.Status409Conflict,
            var c when c.Contains("Validation") => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Description, title: error.Code, statusCode: statusCode);
    }
}
