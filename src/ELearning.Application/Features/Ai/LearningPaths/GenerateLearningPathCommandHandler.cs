using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.LearningPaths;

public sealed class GenerateLearningPathCommandHandler(
    IAiLearningPathService learningPathService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GenerateLearningPathCommand, Result<LearningPathDraftDto>>
{
    private const string Provider = "Local";
    private const string Model = "local-learning-path-v1";
    private const string PromptVersion = "learning-path-generator-v1";

    public async Task<Result<LearningPathDraftDto>> Handle(GenerateLearningPathCommand request, CancellationToken ct)
    {
        var input = new AiLearningPathRequest(
            request.Goal.Trim(),
            string.IsNullOrWhiteSpace(request.CurrentSkills) ? null : request.CurrentSkills.Trim(),
            string.IsNullOrWhiteSpace(request.TargetRole) ? null : request.TargetRole.Trim(),
            request.OrganizationId,
            Math.Clamp(request.MaxCourses, 1, 12));

        var inputHash = ComputeInputHash(input);
        var cacheKey = cacheKeyBuilder.Build("ai", "learning-path", inputHash);

        try
        {
            var draft = await cache.GetOrCreateAsync(
                cacheKey,
                token => learningPathService.GenerateAsync(input, token),
                TimeSpan.FromMinutes(10),
                ct);

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "LearningPathGeneration",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                EstimateTokens(input.Goal + " " + input.CurrentSkills + " " + input.TargetRole)));
            await unitOfWork.SaveChangesAsync(ct);

            return new LearningPathDraftDto(
                Provider,
                Model,
                PromptVersion,
                inputHash,
                draft.Goal,
                draft.TargetRole,
                draft.Confidence,
                draft.EstimatedEffort,
                draft.MissingSkills,
                draft.Courses.Select(x => new LearningPathCourseDto(
                    x.Order,
                    x.CourseId,
                    x.Title,
                    x.Description,
                    x.PriceCents,
                    x.Currency,
                    x.Score,
                    x.EstimatedEffort,
                    x.Reasons)).ToList());
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "LearningPathGeneration",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<LearningPathDraftDto>(Error.Validation("AI.LearningPath", ex.Message));
        }
    }

    private static string ComputeInputHash(AiLearningPathRequest input)
    {
        var raw = string.Join('|', input.Goal, input.CurrentSkills, input.TargetRole, input.OrganizationId, input.MaxCourses, PromptVersion);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4m));
}
