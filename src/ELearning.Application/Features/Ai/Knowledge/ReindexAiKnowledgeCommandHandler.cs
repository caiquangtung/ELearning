using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.Knowledge;

public sealed class ReindexAiKnowledgeCommandHandler(
    IAiKnowledgeIndexingService indexingService,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReindexAiKnowledgeCommand, Result<ReindexAiKnowledgeDto>>
{
    private const string Feature = "RagKnowledgeReindex";
    private const string Provider = "Configured";
    private const string Model = "configured-rag-embedding-v1";
    private const string PromptVersion = "rag-knowledge-index-v1";

    public async Task<Result<ReindexAiKnowledgeDto>> Handle(ReindexAiKnowledgeCommand request, CancellationToken ct)
    {
        var inputHash = ComputeInputHash(request.CourseId);

        try
        {
            var result = await indexingService.ReindexAsync(
                request.CourseId,
                currentUserService.UserId,
                null,
                ct);
            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                Feature,
                Provider,
                Model,
                PromptVersion,
                inputHash,
                null));
            await unitOfWork.SaveChangesAsync(ct);

            return new ReindexAiKnowledgeDto(
                result.JobId,
                result.IndexedCourses,
                result.IndexedChunks,
                result.DeletedStaleChunks);
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                Feature,
                Provider,
                Model,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<ReindexAiKnowledgeDto>(Error.Validation("AI.Knowledge", ex.Message));
        }
    }

    private static string ComputeInputHash(Guid? courseId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{courseId}|{PromptVersion}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
