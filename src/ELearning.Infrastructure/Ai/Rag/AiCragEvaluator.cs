using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Ai;

public sealed class AiCragEvaluator : IAiCragEvaluator
{
    private const decimal AmbiguousFloorThreshold = 0.22m;

    public CragEvaluationResult Evaluate(
        IReadOnlyList<AiChatCitation> citations,
        int totalCandidatesCount,
        decimal? maxScore,
        decimal minAcceptedScore)
    {
        var effectiveMaxScore = maxScore ?? (citations.Count > 0 ? citations.Max(c => c.Score) : 0m);

        // State 1: Correct (High Similarity & Solid Candidates)
        if (citations.Count > 0 && effectiveMaxScore >= minAcceptedScore)
        {
            return new CragEvaluationResult(
                CragEvaluationState.Correct,
                effectiveMaxScore,
                minAcceptedScore,
                citations,
                "High retrieval quality. Grounded in exact lesson content.");
        }

        // State 2: Ambiguous (Borderline Similarity or Partial Candidates)
        if (citations.Count > 0 && effectiveMaxScore >= AmbiguousFloorThreshold)
        {
            return new CragEvaluationResult(
                CragEvaluationState.Ambiguous,
                effectiveMaxScore,
                minAcceptedScore,
                citations,
                "Borderline retrieval quality. Proceeding with broader course context disclosure.");
        }

        // State 3: Incorrect (Low Similarity / Irrelevant / Insufficient Candidates)
        return new CragEvaluationResult(
            CragEvaluationState.Incorrect,
            effectiveMaxScore,
            minAcceptedScore,
            [],
            "Retrieval quality gate rejected context. No sufficiently relevant lesson candidates found.");
    }
}
