using Asp.Versioning;
using ELearning.Application.Features.Videos.GetLessonVideo;
using ELearning.Application.Features.Videos.GetVideoPlayback;
using ELearning.Application.Features.Videos.MarkLessonComplete;
using ELearning.Application.Features.Videos.TrackWatchProgress;
using ELearning.Application.Features.Videos.UploadVideo;
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
[Route("api/v{version:apiVersion}/videos")]
public sealed class VideosController(IMediator mediator) : ControllerBase
{
    [HttpPost("courses/{courseId:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}")]
    [HasPermission(Permissions.Courses.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(
        Guid courseId,
        Guid sectionId,
        Guid lessonId,
        [FromForm] UploadVideoRequest body,
        CancellationToken ct)
    {
        await using var stream = body.File.OpenReadStream();
        var result = await mediator.Send(new UploadVideoCommand(
            courseId,
            sectionId,
            lessonId,
            stream,
            body.File.FileName,
            body.File.ContentType,
            body.DurationSeconds), ct);

        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("{id:guid}/playback")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Playback(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetVideoPlaybackQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("lessons/{lessonId:guid}")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonVideo(Guid lessonId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLessonVideoQuery(lessonId), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("{id:guid}/progress")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackProgress(Guid id, [FromBody] TrackWatchProgressRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new TrackWatchProgressCommand(
            id,
            body.PositionSeconds,
            body.DurationSeconds,
            body.WatchedSeconds), ct);

        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("{id:guid}/complete")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkComplete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new MarkLessonCompleteCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    private IActionResult ProblemFrom(Error error)
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
