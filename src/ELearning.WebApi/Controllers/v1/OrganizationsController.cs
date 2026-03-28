using Asp.Versioning;
using ELearning.Application.Features.Organizations.AddMember;
using ELearning.Application.Features.Organizations.CreateOrganization;
using ELearning.Application.Features.Organizations.GetOrganization;
using ELearning.Application.Features.Organizations.ListOrganizations;
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
[Route("api/v{version:apiVersion}/organizations")]
public class OrganizationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await mediator.Send(new ListOrganizationsQuery(), ct);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrganizationQuery(id), ct);
        return FromResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : FromResult(result);
    }

    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddMemberRequest body,
        CancellationToken ct)
    {
        var cmd = new AddMemberToOrganizationCommand(id, body.UserId, body.OrgRole, body.DepartmentId);
        var result = await mediator.Send(cmd, ct);
        return FromResult(result);
    }

    private IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var c when c.Contains("Conflict") || c.Contains("EmailTaken") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Description, title: error.Code, statusCode: statusCode);
    }
}
