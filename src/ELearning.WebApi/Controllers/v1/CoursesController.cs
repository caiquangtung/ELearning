using Asp.Versioning;
using ELearning.Application.Features.Courses.AddLesson;
using ELearning.Application.Features.Courses.AddSection;
using ELearning.Application.Features.Courses.CreateCourse;
using ELearning.Application.Features.Courses.DeleteCourse;
using ELearning.Application.Features.Courses.GetCourse;
using ELearning.Application.Features.Courses.ListCourses;
using ELearning.Application.Features.Courses.PublishCourse;
using ELearning.Application.Features.Courses.UpdateCourse;
using ELearning.Application.Features.Courses.UploadAsset;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.WebApi.Authorization;
using ELearning.WebApi.Contracts.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("api/v{version:apiVersion}/courses")]
public sealed class CoursesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var parsedStatus = Enum.TryParse(status, true, out Domain.Aggregates.CourseAggregate.CourseStatus s)
            ? s
            : (Domain.Aggregates.CourseAggregate.CourseStatus?)null;

        var result = await mediator.Send(new ListCoursesQuery(page, pageSize, search, parsedStatus), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCourseQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Courses.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCourseCommand(body.Title, body.Description), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Courses.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCourseCommand(id, body.Title, body.Description), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Courses.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCourseCommand(id), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/publish")]
    [HasPermission(Permissions.Courses.Publish)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new PublishCourseCommand(id), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/sections")]
    [HasPermission(Permissions.Courses.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddSection(Guid id, [FromBody] AddSectionRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new AddSectionToCourseCommand(id, body.Title), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("{courseId:guid}/sections/{sectionId:guid}/lessons")]
    [HasPermission(Permissions.Courses.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddLesson(
        Guid courseId,
        Guid sectionId,
        [FromBody] AddLessonRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new AddLessonToSectionCommand(courseId, sectionId, body.Title), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("{courseId:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}/assets")]
    [HasPermission(Permissions.Courses.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadAsset(
        Guid courseId,
        Guid sectionId,
        Guid lessonId,
        [FromForm] UploadAssetRequest body,
        CancellationToken ct)
    {
        await using var stream = body.File.OpenReadStream();
        var cmd = new UploadLessonAssetCommand(
            courseId,
            sectionId,
            lessonId,
            body.AssetType,
            stream,
            body.File.FileName,
            body.File.ContentType);

        var result = await mediator.Send(cmd, ct);
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

