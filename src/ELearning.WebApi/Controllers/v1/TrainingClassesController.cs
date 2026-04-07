using Asp.Versioning;
using ELearning.Application.Features.TrainingClasses.AssignInstructor;
using ELearning.Application.Features.TrainingClasses.CancelSession;
using ELearning.Application.Features.TrainingClasses.CreateTrainingClass;
using ELearning.Application.Features.TrainingClasses.DeleteTrainingClass;
using ELearning.Application.Features.TrainingClasses.GetTrainingClass;
using ELearning.Application.Features.TrainingClasses.ListTrainingClasses;
using ELearning.Application.Features.TrainingClasses.RemoveInstructor;
using ELearning.Application.Features.TrainingClasses.ScheduleSession;
using ELearning.Application.Features.TrainingClasses.UpdateSession;
using ELearning.Application.Features.TrainingClasses.UpdateTrainingClass;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.WebApi.Authorization;
using ELearning.WebApi.Contracts.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("api/v{version:apiVersion}/training-classes")]
public sealed class TrainingClassesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Classes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? courseId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListTrainingClassesQuery(page, pageSize, courseId, search), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Classes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTrainingClassQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Classes.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTrainingClassRequest body, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new CreateTrainingClassCommand(body.CourseId, body.Title, body.MaxLearners),
            ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Classes.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainingClassRequest body, CancellationToken ct = default)
    {
        var result = await mediator.Send(new UpdateTrainingClassCommand(id, body.Title, body.MaxLearners), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Classes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new DeleteTrainingClassCommand(id), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/instructors")]
    [HasPermission(Permissions.Classes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignInstructor(Guid id, [FromBody] AssignInstructorRequest body, CancellationToken ct = default)
    {
        var result = await mediator.Send(new AssignInstructorCommand(id, body.UserId), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpDelete("{id:guid}/instructors/{userId:guid}")]
    [HasPermission(Permissions.Classes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveInstructor(Guid id, Guid userId, CancellationToken ct = default)
    {
        var result = await mediator.Send(new RemoveInstructorCommand(id, userId), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/sessions")]
    [HasPermission(Permissions.Classes.ManageSessions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ScheduleSession(
        Guid id,
        [FromBody] ScheduleSessionRequest body,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<ClassSessionType>(body.SessionType, true, out var sessionType))
            return Problem(Error.Validation("SessionType", "Invalid session type. Use Zoom, Offline, or Vod."));

        var result = await mediator.Send(
            new ScheduleSessionCommand(id, body.Title, sessionType, body.StartUtc, body.EndUtc, body.Location),
            ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPut("{id:guid}/sessions/{sessionId:guid}")]
    [HasPermission(Permissions.Classes.ManageSessions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSession(
        Guid id,
        Guid sessionId,
        [FromBody] UpdateSessionRequest body,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<ClassSessionType>(body.SessionType, true, out var sessionType))
            return Problem(Error.Validation("SessionType", "Invalid session type. Use Zoom, Offline, or Vod."));

        var result = await mediator.Send(
            new UpdateSessionCommand(
                id,
                sessionId,
                body.Title,
                sessionType,
                body.StartUtc,
                body.EndUtc,
                body.Location),
            ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("{id:guid}/sessions/{sessionId:guid}/cancel")]
    [HasPermission(Permissions.Classes.ManageSessions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelSession(Guid id, Guid sessionId, CancellationToken ct = default)
    {
        var result = await mediator.Send(new CancelSessionCommand(id, sessionId), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
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
