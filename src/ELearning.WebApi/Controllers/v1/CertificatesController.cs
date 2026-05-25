using Asp.Versioning;
using ELearning.Application.Features.Certificates.GetCertificate;
using ELearning.Application.Features.Certificates.GetCertificatePdf;
using ELearning.Application.Features.Certificates.IssueCertificate;
using ELearning.Application.Features.Certificates.VerifyCertificate;
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
[Route("api/v{version:apiVersion}/certificates")]
public sealed class CertificatesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [HasPermission(Permissions.Certificates.Issue)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Issue([FromBody] IssueCertificateRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new IssueCertificateCommand(
                body.UserId,
                body.CourseId,
                body.TrainingClassId,
                body.QuizAttemptId,
                body.LearnerName,
                body.CourseTitle,
                body.AttendancePercent,
                body.ProgressPercent,
                body.QuizPassed,
                body.ExpiresAt),
            ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [HasPermission(Permissions.Certificates.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCertificateQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}/pdf")]
    [Authorize]
    [HasPermission(Permissions.Certificates.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCertificatePdfQuery(id), ct);
        return result.IsSuccess
            ? File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : Problem(result.Error);
    }

    [HttpGet("verify/{verificationCode}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Verify(string verificationCode, CancellationToken ct)
    {
        var result = await mediator.Send(new VerifyCertificateQuery(verificationCode), ct);
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
