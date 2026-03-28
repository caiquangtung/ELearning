using Asp.Versioning;
using ELearning.Application.Features.Identity.Common;
using ELearning.Application.Features.Identity.GetMe;
using ELearning.Application.Features.Identity.Login;
using ELearning.Application.Features.Identity.RefreshToken;
using ELearning.Application.Features.Identity.Register;
using ELearning.Application.Features.Identity.UpdateProfile;
using ELearning.Core.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/identity")]
public class IdentityController(IMediator mediator) : ControllerBase
{
    /// <summary>Register a new learner account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMe), result.Value)
            : Problem(result.Error);
    }

    /// <summary>Authenticate with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>Exchange an expired access token + refresh token for a new pair.</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>Return the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMeQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>Update the authenticated user's profile (name).</summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var c when c.Contains("Conflict") || c.Contains("EmailTaken") => StatusCodes.Status409Conflict,
            var c when c.Contains("Suspended", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Description, title: error.Code, statusCode: statusCode);
    }
}
