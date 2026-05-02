using Asp.Versioning;
using ELearning.Application.Features.Promotions.Campaigns.AddRule;
using ELearning.Application.Features.Promotions.Campaigns.CreateCampaign;
using ELearning.Application.Features.Promotions.Campaigns.CreateCoupon;
using ELearning.Application.Features.Promotions.Campaigns.GetCampaign;
using ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("api/v{version:apiVersion}/campaigns")]
public sealed class CampaignsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid? organizationId, [FromQuery] bool includeGlobal = true, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListCampaignsQuery(organizationId, includeGlobal, take), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCampaignQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("{id:guid}/rules")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddRule(Guid id, [FromBody] AddItemPercentOffRuleCommand body, CancellationToken ct)
    {
        var cmd = body with { CampaignId = id };
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("{id:guid}/coupons")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCoupon(Guid id, [FromBody] CreateCouponCommand body, CancellationToken ct)
    {
        var cmd = body with { CampaignId = id };
        var result = await mediator.Send(cmd, ct);
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

