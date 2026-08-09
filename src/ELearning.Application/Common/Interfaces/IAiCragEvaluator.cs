namespace ELearning.Application.Common.Interfaces;

public enum CragEvaluationState
{
    Correct,
    Ambiguous,
    Incorrect
}

public sealed record CragEvaluationResult(
    CragEvaluationState State,
    decimal MaxScore,
    decimal MinAcceptedScore,
    IReadOnlyList<AiChatCitation> Citations,
    string SummaryReason);

public interface IAiCragEvaluator
{
    CragEvaluationResult Evaluate(
        IReadOnlyList<AiChatCitation> citations,
        int totalCandidatesCount,
        decimal? maxScore,
        decimal minAcceptedScore);
}
