namespace ELearning.Application.Common.Interfaces;

public sealed record AiChatMessageContext(
    string Role,
    string Content);

public interface IAiQueryRewriter
{
    Task<string> RewriteQueryAsync(
        string question,
        IReadOnlyList<AiChatMessageContext> chatHistory,
        CancellationToken ct = default);
}
