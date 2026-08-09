namespace ELearning.Application.Common.Interfaces;

public sealed record AiQueryDecompositionResult(
    string OriginalQuery,
    IReadOnlyList<string> SubQueries,
    bool IsDecomposed,
    string? Reason);

public interface IAiQueryDecomposer
{
    AiQueryDecompositionResult DecomposeQuery(string question);
}
