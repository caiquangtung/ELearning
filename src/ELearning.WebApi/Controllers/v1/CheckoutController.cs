using Asp.Versioning;
using ELearning.Application.Features.Promotions.QuoteCheckout;
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
[Route("api/v{version:apiVersion}/checkout")]
public sealed class CheckoutController(IMediator mediator) : ControllerBase
{
    [HttpPost("quote")]
    [HasPermission(Permissions.Commerce.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Quote([FromBody] QuoteCheckoutRequest body, CancellationToken ct)
    {
        var q = new QuoteCheckoutQuery(
            body.BuyerUserId,
            body.OrganizationId,
            body.Currency,
            body.Items.Select(i => new QuoteCheckoutItem(i.ItemType, i.ReferenceId, i.Quantity)).ToList(),
            body.CouponCode);

        var result = await mediator.Send(q, ct);
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

