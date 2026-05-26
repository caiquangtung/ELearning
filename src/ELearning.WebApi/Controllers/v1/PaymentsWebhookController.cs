using Asp.Versioning;
using ELearning.Application.Common.Options;
using ELearning.Application.Features.Orders.CompletePayment;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.WebApi.Authorization;
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
public sealed class PaymentsWebhookController(
    IMediator mediator,
    IOptions<PaymentOptions> paymentOptions,
    IIdempotencyStore idempotencyStore,
    ICacheKeyBuilder cacheKeyBuilder,
    IAuditLogService auditLogs)
    : ControllerBase
{
    [HttpPost("webhook")]
    [WebhookSecret("Payments:WebhookSecret", "X-Payments-Webhook-Secret")]
    [RequestSizeLimit(65_536)]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.TransactionId))
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Payment.Webhook",
                "PaymentTransaction",
                null,
                "Failure",
                new Dictionary<string, string> { ["reason"] = "missing_transaction_id" }), ct);
            return BadRequest();
        }

        var transactionId = body.TransactionId.Trim();
        var provider = paymentOptions.Value.Provider ?? "unknown";
        var key = cacheKeyBuilder.Build("payment", "webhook", provider, transactionId);
        var ttl = TimeSpan.FromHours(24);

        var begin = await idempotencyStore.TryBeginAsync(key, ttl, ct);
        if (begin.Status == IdempotencyBeginStatus.Completed)
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Payment.Webhook",
                "PaymentTransaction",
                transactionId,
                "Skipped",
                new Dictionary<string, string>
                {
                    ["reason"] = "already_completed",
                    ["provider"] = provider
                }), ct);
            return Ok();
        }
        if (begin.Status == IdempotencyBeginStatus.InProgress)
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Payment.Webhook",
                "PaymentTransaction",
                transactionId,
                "Conflict",
                new Dictionary<string, string>
                {
                    ["reason"] = "in_progress",
                    ["provider"] = provider
                }), ct);
            return Conflict(new { message = "Payment webhook is already being processed." });
        }
        if (begin.Status == IdempotencyBeginStatus.Unavailable)
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Payment.Webhook",
                "PaymentTransaction",
                transactionId,
                "Failure",
                new Dictionary<string, string>
                {
                    ["reason"] = "idempotency_unavailable",
                    ["provider"] = provider
                }), ct);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = begin.FailureReason ?? "Idempotency store is unavailable." });
        }

        var result = await mediator.Send(new CompleteOrderPaymentCommand(transactionId), ct);
        if (result.IsSuccess)
        {
            await idempotencyStore.CompleteAsync(key, ttl, ct);
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Payment.Webhook",
                "PaymentTransaction",
                transactionId,
                "Success",
                new Dictionary<string, string> { ["provider"] = provider }), ct);
            return Ok();
        }

        await idempotencyStore.FailAsync(key, TimeSpan.FromMinutes(10), ct);
        await auditLogs.WriteAsync(new AuditLogEntry(
            "Payment.Webhook",
            "PaymentTransaction",
            transactionId,
            "Failure",
            new Dictionary<string, string>
            {
                ["provider"] = provider,
                ["errorCode"] = result.Error.Code
            }), ct);
        return ProblemFrom(result.Error);
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
