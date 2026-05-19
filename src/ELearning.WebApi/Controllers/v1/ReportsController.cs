using Asp.Versioning;
using ELearning.Application.Features.Reports.GetAdminDashboard;
using ELearning.Application.Features.Reports.GetCourseAnalytics;
using ELearning.Application.Features.Reports.GetInstructorDashboard;
using ELearning.Application.Features.Reports.GetOrganizationAnalytics;
using ELearning.Application.Features.Reports.GetStudentDashboard;
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
[Route("api/v{version:apiVersion}/reports")]
public sealed class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard/admin")]
    [HasPermission(Permissions.Admin.Access)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AdminDashboard(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminDashboardQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("dashboard/student")]
    [HasPermission(Permissions.Reports.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StudentDashboard(CancellationToken ct)
    {
        var result = await mediator.Send(new GetStudentDashboardQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("dashboard/instructor")]
    [HasPermission(Permissions.Reports.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InstructorDashboard(CancellationToken ct)
    {
        var result = await mediator.Send(new GetInstructorDashboardQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("courses/{courseId:guid}")]
    [HasPermission(Permissions.Reports.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CourseAnalytics(Guid courseId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCourseAnalyticsQuery(courseId), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("organizations/{organizationId:guid}")]
    [HasPermission(Permissions.Reports.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> OrganizationAnalytics(Guid organizationId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrganizationAnalyticsQuery(organizationId), ct);
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
