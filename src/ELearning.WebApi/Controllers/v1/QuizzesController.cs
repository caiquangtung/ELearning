using Asp.Versioning;
using ELearning.Application.Features.Quizzes.AddQuestion;
using ELearning.Application.Features.Quizzes.CreateQuiz;
using ELearning.Application.Features.Quizzes.DeleteQuiz;
using ELearning.Application.Features.Quizzes.GetQuiz;
using ELearning.Application.Features.Quizzes.GetQuizAnalytics;
using ELearning.Application.Features.Quizzes.GetQuizResults;
using ELearning.Application.Features.Quizzes.GradeAttempt;
using ELearning.Application.Features.Quizzes.ListQuizzes;
using ELearning.Application.Features.Quizzes.PublishQuiz;
using ELearning.Application.Features.Quizzes.RemoveQuestion;
using ELearning.Application.Features.Quizzes.StartAttempt;
using ELearning.Application.Features.Quizzes.SubmitAttempt;
using ELearning.Application.Features.Quizzes.UpdateQuestion;
using ELearning.Application.Features.Quizzes.UpdateQuiz;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.QuizAggregate;
using ELearning.WebApi.Authorization;
using ELearning.WebApi.Contracts.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("api/v{version:apiVersion}/quizzes")]
public sealed class QuizzesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListQuizzesRequest query, CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListQuizzesQuery(query.Page, query.PageSize, query.Search, query.Status), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetQuizQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [HasPermission(Permissions.Quizzes.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateQuizRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateQuizCommand(body.CourseId, body.LessonId, body.Title, body.Description, body.TimeLimitMinutes, body.PassingScore),
            ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Quizzes.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuizRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateQuizCommand(id, body.Title, body.Description, body.TimeLimitMinutes, body.PassingScore),
            ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Quizzes.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteQuizCommand(id), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/publish")]
    [HasPermission(Permissions.Quizzes.Publish)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new PublishQuizCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("{id:guid}/questions")]
    [HasPermission(Permissions.Quizzes.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddQuestion(Guid id, [FromBody] AddQuestionRequest body, CancellationToken ct)
    {
        var options = body.Options?.Select(o => new AddQuestionOptionDto(o.Text, o.IsCorrect, o.SortOrder)).ToList()
            ?? new List<AddQuestionOptionDto>();

        var result = await mediator.Send(
            new AddQuestionCommand(id, body.Text, body.Type, body.Points, body.SortOrder, options),
            ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPut("{quizId:guid}/questions/{questionId:guid}")]
    [HasPermission(Permissions.Quizzes.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQuestion(
        Guid quizId,
        Guid questionId,
        [FromBody] UpdateQuestionRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateQuestionCommand(quizId, questionId, body.Text, body.Type, body.Points, body.SortOrder),
            ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("{quizId:guid}/questions/{questionId:guid}")]
    [HasPermission(Permissions.Quizzes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveQuestion(Guid quizId, Guid questionId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveQuestionCommand(quizId, questionId), ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error);
    }

    [HttpPost("{id:guid}/attempts")]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> StartAttempt(Guid id, [FromBody] StartAttemptRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new StartAttemptCommand(id, body.UserId), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetAttempt), new { attemptId = result.Value.Id }, result.Value) : Problem(result.Error);
    }

    [HttpGet("attempts/{attemptId:guid}")]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttempt(Guid attemptId, [FromQuery] GetAttemptRequest query, CancellationToken ct)
    {
        var result = await mediator.Send(new GetQuizResultsQuery(attemptId, query.UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitAttempt(
        Guid attemptId,
        [FromBody] SubmitAttemptRequest body,
        CancellationToken ct)
    {
        var answers = body.Answers?.Select(a => new AnswerSubmission(a.QuestionId, a.SelectedOptionId, a.TextAnswer)).ToList()
            ?? new List<AnswerSubmission>();

        var result = await mediator.Send(new SubmitAttemptCommand(attemptId, body.UserId, answers), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("attempts/{attemptId:guid}/grade")]
    [HasPermission(Permissions.Quizzes.Grade)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GradeAttempt(
        Guid attemptId,
        [FromBody] GradeAttemptRequest body,
        CancellationToken ct)
    {
        var grades = body.Grades?.Select(g => new QuestionGrade(g.QuestionId, g.Score, g.IsCorrect)).ToList()
            ?? new List<QuestionGrade>();

        var result = await mediator.Send(new GradeAttemptCommand(attemptId, grades), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}/analytics")]
    [HasPermission(Permissions.Quizzes.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetQuizAnalyticsQuery(id), ct);
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
