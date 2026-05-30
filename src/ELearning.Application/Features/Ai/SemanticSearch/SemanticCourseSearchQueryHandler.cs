using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.SemanticSearch;

public sealed class SemanticCourseSearchQueryHandler(
    IAiSemanticSearchService searchService,
    ICacheService cache,
    ICacheKeyBuilder cacheKeyBuilder,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SemanticCourseSearchQuery, Result<SemanticCourseSearchDto>>
{
    private const string Provider = "Local";
    private const string Model = "local-token-embedding-v1";
    private const string PromptVersion = "semantic-course-search-v1";

    public async Task<Result<SemanticCourseSearchDto>> Handle(SemanticCourseSearchQuery request, CancellationToken ct)
    {
        var query = request.Query.Trim();
        var limit = Math.Clamp(request.Limit, 1, 20);
        var inputHash = ComputeInputHash(query, limit);
        var cacheKey = cacheKeyBuilder.Build("ai", "semantic-search", inputHash, limit.ToString());

        try
        {
            var results = await cache.GetOrCreateAsync(
                cacheKey,
                token => searchService.SearchCoursesAsync(query, limit, token),
                TimeSpan.FromMinutes(5),
                ct);

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                "SemanticCourseSearch",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                tokenEstimate: EstimateTokens(query)));
            await unitOfWork.SaveChangesAsync(ct);

            return new SemanticCourseSearchDto(
                Provider,
                Model,
                PromptVersion,
                inputHash,
                results.Select(x => new SemanticCourseSearchResultDto(
                    x.CourseId,
                    x.Title,
                    x.Description,
                    x.PriceCents,
                    x.Currency,
                    x.CreatedAt,
                    x.Score,
                    x.MatchedConcepts,
                    x.Reasons)).ToList());
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                "SemanticCourseSearch",
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<SemanticCourseSearchDto>(Error.Validation("AI.SemanticSearch", ex.Message));
        }
    }

    private static string ComputeInputHash(string query, int limit)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{query}|{limit}|{PromptVersion}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4m));
}
