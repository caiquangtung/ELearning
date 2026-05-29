using Asp.Versioning;
using ELearning.Application.Features.Ai.QuizQuestionGeneration;
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
[Route("api/v{version:apiVersion}/ai")]
public sealed class AiController(IMediator mediator) : ControllerBase
{
    [HttpPost("quizzes/generate-questions")]
    [HasPermission(Permissions.Ai.Use)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateQuizQuestions([FromBody] GenerateQuizQuestionsRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GenerateQuizQuestionsCommand(
                body.CourseId,
                body.LessonId,
                body.QuestionCount,
                body.Difficulty,
                body.QuestionTypes),
            ct);

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
