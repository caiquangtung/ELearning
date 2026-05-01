using Asp.Versioning;
using ELearning.Application.Features.Licenses.AssignLicense;
using ELearning.Application.Features.Licenses.CreateLicensePool;
using ELearning.Application.Features.Licenses.GetLicensePool;
using ELearning.Application.Features.Licenses.GetLicenseUsage;
using ELearning.Application.Features.Licenses.ListLicensePools;
using ELearning.Application.Features.Licenses.RevokeLicense;
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
public sealed class LicensePoolsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Licenses.Read)]
    [Route("api/v{version:apiVersion}/organizations/{organizationId:guid}/license-pools")]
    public async Task<IActionResult> List(Guid organizationId, CancellationToken ct)
    {
        var result = await mediator.Send(new ListLicensePoolsQuery(organizationId), ct);
        return FromResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Licenses.Assign)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Route("api/v{version:apiVersion}/organizations/{organizationId:guid}/license-pools")]
    public async Task<IActionResult> Create(Guid organizationId, [FromBody] CreateLicensePoolRequest body, CancellationToken ct)
    {
        var cmd = new CreateLicensePoolCommand(organizationId, body.Name, body.TotalSeats, body.ExpiresAt);
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : FromResult(result);
    }

    [HttpGet("api/v{version:apiVersion}/license-pools/{id:guid}")]
    [HasPermission(Permissions.Licenses.Read)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLicensePoolQuery(id), ct);
        return FromResult(result);
    }

    [HttpGet("api/v{version:apiVersion}/license-pools/{id:guid}/usage")]
    [HasPermission(Permissions.Licenses.Read)]
    public async Task<IActionResult> Usage(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLicenseUsageQuery(id), ct);
        return FromResult(result);
    }

    [HttpPost("api/v{version:apiVersion}/license-pools/{id:guid}/assignments")]
    [HasPermission(Permissions.Licenses.Assign)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignLicenseRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new AssignLicenseCommand(id, body.UserId), ct);
        return FromResult(result);
    }

    [HttpDelete("api/v{version:apiVersion}/license-pools/{id:guid}/assignments/{userId:guid}")]
    [HasPermission(Permissions.Licenses.Assign)]
    public async Task<IActionResult> Revoke(Guid id, Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeLicenseCommand(id, userId), ct);
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

