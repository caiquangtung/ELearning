namespace ELearning.Application.Common.Interfaces;

public interface IAiKnowledgeRetriever
{
    Task<AiKnowledgeRetrievalResult> RetrieveAsync(
        AiKnowledgeRetrievalRequest request,
        CancellationToken ct = default);
}

public sealed record AiKnowledgeRetrievalRequest(
    Guid UserId,
    IReadOnlyCollection<string> UserRoles,
    string Question,
    Guid? CourseId);

public sealed record AiKnowledgeRetrievalResult(
    IReadOnlyList<AiChatCitation> Citations,
    int RetrievedCount,
    decimal? MaxScore,
    decimal MinAcceptedScore,
    string VectorProvider,
    string VectorModel,
    int VectorDimensions,
    long ElapsedMilliseconds);
