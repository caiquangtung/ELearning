using Asp.Versioning;
using ELearning.Application.Common.Options;
using ELearning.Application.Features.Orders.CompletePayment;
using ELearning.Core.Common;
using ELearning.WebApi.Contracts.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/payments")]
public sealed class PaymentsWebhookController(IMediator mediator, IOptions<PaymentOptions> paymentOptions)
    : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookRequest body, CancellationToken ct)
    {
        var secret = paymentOptions.Value.WebhookSecret;
        if (!string.IsNullOrEmpty(secret))
        {
            if (!Request.Headers.TryGetValue("X-Payments-Webhook-Secret", out var hdr)
                || hdr.Count == 0
                || hdr.ToString() != secret)
                return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(body.TransactionId))
            return BadRequest();

        var result = await mediator.Send(new CompleteOrderPaymentCommand(body.TransactionId.Trim()), ct);
        return result.IsSuccess ? Ok() : ProblemFrom(result.Error);
    }

    private IActionResult ProblemFrom(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Conflict") => StatusCodes.Status409Conflict,
            var c when c.Contains("Validation") => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Description, title: error.Code, statusCode: statusCode);
    }
}
