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
    public async Task<Result<LearningPathDraftDto>> Handle(GenerateLearningPathCommand request, CancellationToken ct)
    {
        var input = new AiLearningPathRequest(
            request.Goal.Trim(),
            string.IsNullOrWhiteSpace(request.CurrentSkills) ? null : request.CurrentSkills.Trim(),
            string.IsNullOrWhiteSpace(request.TargetRole) ? null : request.TargetRole.Trim(),
            request.OrganizationId,
            Math.Clamp(request.MaxCourses, 1, 12));

        var inputHash = ComputeInputHash(input);
        var cacheKey = cacheKeyBuilder.Build("ai", "learning-path", learningPathService.CacheVariant, inputHash);

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
                draft.Provider,
                draft.Model,
                draft.PromptVersion,
                inputHash,
                draft.TokenEstimate));
            await unitOfWork.SaveChangesAsync(ct);

            return new LearningPathDraftDto(
                draft.Provider,
                draft.Model,
                draft.PromptVersion,
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
                "AI",
                "unknown",
                "learning-path-generator-v1",
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<LearningPathDraftDto>(Error.Validation("AI.LearningPath", ex.Message));
        }
    }

    private static string ComputeInputHash(AiLearningPathRequest input)
    {
        var raw = string.Join('|', input.Goal, input.CurrentSkills, input.TargetRole, input.OrganizationId, input.MaxCourses);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
