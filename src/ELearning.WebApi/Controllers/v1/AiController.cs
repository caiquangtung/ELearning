using Asp.Versioning;
using ELearning.Application.Features.Ai.CourseRecommendations;
using ELearning.Application.Features.Ai.EssayGrading;
using ELearning.Application.Features.Ai.Chat;
using ELearning.Application.Features.Ai.Knowledge;
using ELearning.Application.Features.Ai.LearnerRisk;
using ELearning.Application.Features.Ai.LearningPaths;
using ELearning.Application.Features.Ai.QuizQuestionGeneration;
using ELearning.Application.Features.Ai.RagEvaluations;
using ELearning.Application.Features.Ai.SemanticSearch;
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
    [HttpGet("recommendations/courses")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourseRecommendations([FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCourseRecommendationsQuery(limit), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("search/courses")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCourses([FromQuery] string q, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new SemanticCourseSearchQuery(q, limit), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("learning-paths/generate")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateLearningPath([FromBody] GenerateLearningPathRequestDto body, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GenerateLearningPathCommand(
                body.Goal,
                body.CurrentSkills,
                body.TargetRole,
                body.OrganizationId,
                body.MaxCourses),
            ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("chat/sessions")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateChatSession([FromBody] CreateAiChatSessionRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAiChatSessionCommand(body.CourseId, body.Title), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("chat/sessions")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListChatSessions(CancellationToken ct)
    {
        var result = await mediator.Send(new ListAiChatSessionsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("chat/sessions/{sessionId:guid}/messages")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatMessages(Guid sessionId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAiChatMessagesQuery(sessionId), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("chat/sessions/{sessionId:guid}/messages")]
    [HasPermission(Permissions.Courses.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendChatMessage(Guid sessionId, [FromBody] SendAiChatMessageRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new SendAiChatMessageCommand(sessionId, body.Message), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("knowledge/reindex")]
    [HasPermission(Permissions.Ai.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReindexKnowledge([FromBody] ReindexAiKnowledgeRequest body, CancellationToken ct)
    {
        var result = await mediator.Send(new ReindexAiKnowledgeCommand(body.CourseId), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("knowledge/status")]
    [HasPermission(Permissions.Ai.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKnowledgeStatus(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAiKnowledgeStatusQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpPost("rag/evaluations/run")]
    [HasPermission(Permissions.Ai.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunRagEvaluation(CancellationToken ct)
    {
        var result = await mediator.Send(new RunRagEvaluationCommand(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("rag/evaluations")]
    [HasPermission(Permissions.Ai.Manage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRagEvaluations(CancellationToken ct)
    {
        var result = await mediator.Send(new ListRagEvaluationsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

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

    [HttpPost("quizzes/attempts/{attemptId:guid}/grade-suggestions")]
    [HasPermission(Permissions.Quizzes.Grade)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestEssayGrades(
        Guid attemptId,
        [FromBody] SuggestEssayGradesRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestEssayGradesCommand(attemptId, body.Rubric), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("learners/{userId:guid}/risk")]
    [HasPermission(Permissions.Ai.Use)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLearnerRisk(Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLearnerRiskQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    [HttpGet("organizations/{organizationId:guid}/risk-report")]
    [HasPermission(Permissions.Organizations.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationRiskReport(Guid organizationId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrganizationRiskReportQuery(organizationId), ct);
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
