using ELearning.Application.Common.Interfaces;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Knowledge;

public sealed class GetAiKnowledgeStatusQueryHandler(IAiKnowledgeIndexingService indexingService)
    : IRequestHandler<GetAiKnowledgeStatusQuery, Result<AiKnowledgeStatusDto>>
{
    public async Task<Result<AiKnowledgeStatusDto>> Handle(GetAiKnowledgeStatusQuery request, CancellationToken ct)
    {
        var status = await indexingService.GetStatusAsync(ct);
        return new AiKnowledgeStatusDto(
            status.TotalChunks,
            status.VectorizedChunks,
            status.IndexedCourses,
            status.QueuedJobs,
            status.InProgressJobs,
            status.FailedJobs,
            status.FailedAiRequests,
            status.VectorDimensions,
            status.VectorProvider,
            status.VectorModel,
            status.LastJob is null ? null : ToDto(status.LastJob),
            status.RecentJobs.Select(ToDto).ToList(),
            status.LastEvaluation is null ? null : ToDto(status.LastEvaluation),
            status.RecentEvaluations.Select(ToDto).ToList());
    }

    private static AiKnowledgeReindexJobDto ToDto(AiKnowledgeReindexJobSummary job) =>
        new(
            job.Id,
            job.CourseId,
            job.Status,
            job.RequestedByUserId,
            job.StartedAt,
            job.CompletedAt,
            job.IndexedCourses,
            job.IndexedChunks,
            job.DeletedStaleChunks,
            job.Error,
            job.CreatedAt);

    private static AiRagEvaluationRunDto ToDto(AiRagEvaluationRunSummary run) =>
        new(
            run.Id,
            run.Status,
            run.RequestedByUserId,
            run.DatasetVersion,
            run.TotalCases,
            run.PassedCases,
            run.RetrievalHitRate,
            run.CitationValidityRate,
            run.RefusalAccuracyRate,
            run.GroundednessRate,
            run.Error,
            run.StartedAt,
            run.CompletedAt,
            run.CreatedAt);
}
