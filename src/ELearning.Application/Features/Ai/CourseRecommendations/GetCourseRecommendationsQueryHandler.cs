using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.CourseRecommendations;

public sealed class GetCourseRecommendationsQueryHandler(
    IAiCourseRecommendationService recommendationService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetCourseRecommendationsQuery, Result<CourseRecommendationsDto>>
{
    private const string Provider = "Local";
    private const string Model = "local-hybrid-recommender-v1";
    private const string PromptVersion = "course-recommendation-v1";

    public async Task<Result<CourseRecommendationsDto>> Handle(GetCourseRecommendationsQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<CourseRecommendationsDto>(Error.Unauthorized());

        var limit = Math.Clamp(request.Limit, 1, 20);
        var inputHash = ComputeInputHash(userId.Value, limit);
        var cacheKey = cacheKeyBuilder.Build("ai", "recommendations", "courses", userId.Value.ToString("N"), limit.ToString());

        try
        {
            var recommendations = await cache.GetOrCreateAsync(
                cacheKey,
                token => recommendationService.RecommendAsync(userId.Value, limit, token),
                TimeSpan.FromMinutes(5),
                ct);

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                userId,
                "CourseRecommendation",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                tokenEstimate: null));
            await unitOfWork.SaveChangesAsync(ct);

            return new CourseRecommendationsDto(
                Provider,
                Model,
                PromptVersion,
                inputHash,
                recommendations.Select(x => new CourseRecommendationDto(
                    x.CourseId,
                    x.Title,
                    x.Description,
                    x.PriceCents,
                    x.Currency,
                    x.CreatedAt,
                    x.Score,
                    x.IsFallback,
                    x.Reasons,
                    x.Signals)).ToList());
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                userId,
                "CourseRecommendation",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Failure<CourseRecommendationsDto>(Error.Validation("AI.Recommendations", ex.Message));
        }
    }

    private static string ComputeInputHash(Guid userId, int limit)
    {
        var raw = string.Join('|', userId, limit, PromptVersion);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
