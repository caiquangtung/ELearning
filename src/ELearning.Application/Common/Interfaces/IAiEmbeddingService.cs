namespace ELearning.Application.Common.Interfaces;

public interface IAiEmbeddingService
{
    IReadOnlyDictionary<string, decimal> Embed(string text);
    decimal CosineSimilarity(IReadOnlyDictionary<string, decimal> left, IReadOnlyDictionary<string, decimal> right);
    IReadOnlyList<string> TopSharedTerms(
        IReadOnlyDictionary<string, decimal> left,
        IReadOnlyDictionary<string, decimal> right,
        int limit);
}
