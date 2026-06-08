namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeRetriever
{
    Task<IReadOnlyList<AiChatCitation>> RetrieveAsync(
        AiKnowledgeRetrievalRequest request,
        CancellationToken ct = default);
}

public sealed record AiKnowledgeRetrievalRequest(
    Guid UserId,
    IReadOnlyCollection<string> UserRoles,
    string Question,
    Guid? CourseId);
