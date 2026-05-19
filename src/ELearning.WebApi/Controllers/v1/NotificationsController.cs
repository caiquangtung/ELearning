using Asp.Versioning;
using ELearning.Application.Features.Notifications.GetUnreadCount;
using ELearning.Application.Features.Notifications.ListMyNotifications;
using ELearning.Application.Features.Notifications.MarkNotificationRead;
using ELearning.Application.Features.Notifications.SendAnnouncement;
using ELearning.Application.Features.Notifications.SendEmail;
using ELearning.Application.Features.Notifications.SendNotification;
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
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Notifications.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListNotificationsRequest query, CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListMyNotificationsQuery(query.Page, query.PageSize, query.UnreadOnly), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("unread-count")]
    [HasPermission(Permissions.Notifications.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnreadNotificationCountQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SendNotificationCommand(body.UserId, body.Title, body.Body, body.Type, body.ActionUrl),
            ct);

        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("announcements")]
    [HasPermission(Permissions.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> SendAnnouncement([FromBody] SendAnnouncementRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SendAnnouncementCommand(
                body.RecipientUserIds,
                body.Subject,
                body.Body,
                body.Scope,
                body.OrganizationId,
                body.CourseId,
                body.TrainingClassId,
                body.ActionUrl),
            ct);

        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("email")]
    [HasPermission(Permissions.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new SendEmailCommand(body.To, body.Subject, body.Body), ct);
        return result.IsSuccess ? NoContent() : ProblemFrom(result.Error);
    }

    [HttpPost("{id:guid}/read")]
    [HasPermission(Permissions.Notifications.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new MarkNotificationReadCommand(id), ct);
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
