using Asp.Versioning;
using ELearning.Application.Features.Users.AssignRoles;
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
[Route("api/v{version:apiVersion}/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPut("{userId:guid}/roles")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignRoles(
        Guid userId,
        [FromBody] AssignRolesRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new AssignRolesToUserCommand(userId, body.Roles), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var c when c.Contains("Conflict") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Description, title: error.Code, statusCode: statusCode);
    }
}
