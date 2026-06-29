using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Ai.Knowledge;

namespace ELearning.Application.Features.Ai.RagEvaluations;

internal static class RagEvaluationMapper
{
    public static AiRagEvaluationRunDto ToDto(AiRagEvaluationRunSummary run) =>
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
