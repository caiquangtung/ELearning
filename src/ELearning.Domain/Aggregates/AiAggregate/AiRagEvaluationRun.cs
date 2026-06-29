using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.AiAggregate;

public sealed class AiRagEvaluationRun : AuditableAggregateRoot
{
    private AiRagEvaluationRun() { }

    public AiRagEvaluationRunStatus Status { get; private set; }
    public Guid? RequestedByUserId { get; private set; }
    public string DatasetVersion { get; private set; } = default!;
    public int TotalCases { get; private set; }
    public int PassedCases { get; private set; }
    public decimal RetrievalHitRate { get; private set; }
    public decimal CitationValidityRate { get; private set; }
    public decimal RefusalAccuracyRate { get; private set; }
    public decimal GroundednessRate { get; private set; }
    public string? Error { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public static AiRagEvaluationRun Succeeded(
        Guid? requestedByUserId,
        string datasetVersion,
        int totalCases,
        int passedCases,
        decimal retrievalHitRate,
        decimal citationValidityRate,
        decimal refusalAccuracyRate,
        decimal groundednessRate,
        DateTime startedAt)
    {
        var now = DateTime.UtcNow;
        return new AiRagEvaluationRun
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = requestedByUserId,
            Status = AiRagEvaluationRunStatus.Succeeded,
            DatasetVersion = NormalizeDatasetVersion(datasetVersion),
            TotalCases = Math.Max(0, totalCases),
            PassedCases = Math.Clamp(passedCases, 0, Math.Max(0, totalCases)),
            RetrievalHitRate = ClampRate(retrievalHitRate),
            CitationValidityRate = ClampRate(citationValidityRate),
            RefusalAccuracyRate = ClampRate(refusalAccuracyRate),
            GroundednessRate = ClampRate(groundednessRate),
            StartedAt = startedAt,
            CompletedAt = now,
            CreatedAt = now
        };
    }

    public static AiRagEvaluationRun Failed(
        Guid? requestedByUserId,
        string datasetVersion,
        string error,
        DateTime startedAt)
    {
        var now = DateTime.UtcNow;
        return new AiRagEvaluationRun
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = requestedByUserId,
            Status = AiRagEvaluationRunStatus.Failed,
            DatasetVersion = NormalizeDatasetVersion(datasetVersion),
            Error = string.IsNullOrWhiteSpace(error) ? "RAG evaluation failed." : error.Trim(),
            StartedAt = startedAt,
            CompletedAt = now,
            CreatedAt = now
        };
    }

    private static string NormalizeDatasetVersion(string value) =>
        string.IsNullOrWhiteSpace(value) ? "rag-golden-v1" : value.Trim();

    private static decimal ClampRate(decimal value) =>
        Math.Round(Math.Clamp(value, 0m, 1m), 4);
}
