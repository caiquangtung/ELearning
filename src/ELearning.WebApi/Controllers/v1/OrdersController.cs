using Asp.Versioning;
using ELearning.Application.Features.Orders.CreateOrder;
using ELearning.Application.Features.Orders.GetInvoice;
using ELearning.Application.Features.Orders.GetOrder;
using ELearning.Application.Features.Orders.ListMyOrders;
using ELearning.Application.Features.Orders.PayOrder;
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
[Route("api/v{version:apiVersion}/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    [HasPermission(Permissions.Commerce.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMy([FromQuery] ListMyOrdersRequest query, CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListMyOrdersQuery(query.BuyerUserId, query.Page, query.PageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Commerce.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}/invoice")]
    [HasPermission(Permissions.Commerce.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInvoiceByOrderQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Commerce.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest body, CancellationToken ct)
    {
        var cmd = new CreateOrderCommand(
            body.BuyerUserId,
            body.OrganizationId,
            body.Currency,
            body.Items.Select(i => new CreateOrderItem(i.ItemType, i.ReferenceId, i.Quantity, i.UnitPriceCents)).ToList(),
            body.DiscountCents,
            body.CouponCode);

        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpPost("{id:guid}/pay")]
    [HasPermission(Permissions.Commerce.Pay)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Pay(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new PayOrderCommand(id), ct);
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
