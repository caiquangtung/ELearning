namespace ELearning.Application.Common.Interfaces;

public interface IAiRagEvaluationService
{
    Task<AiRagEvaluationRunSummary> RunAsync(
        Guid? requestedByUserId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken ct = default);

    Task<IReadOnlyList<AiRagEvaluationRunSummary>> ListAsync(CancellationToken ct = default);
}
