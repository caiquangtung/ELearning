namespace ELearning.Application.Common.Interfaces;

public sealed record AiAccessibleCourse(
    Guid Id,
    string Title);

public interface IAiRagChatService
{
    Task<AiChatSessionSummary> CreateSessionAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        Guid? courseId,
        string? title,
        CancellationToken ct = default);

    Task<IReadOnlyList<AiAccessibleCourse>> GetAccessibleCoursesAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken ct = default);

    Task<IReadOnlyList<AiChatSessionSummary>> ListSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<AiChatMessageItem>> GetMessagesAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<AiChatAnswer> SendMessageAsync(
        Guid userId,
        IReadOnlyCollection<string> userRoles,
        Guid sessionId,
        string message,
        CancellationToken ct = default);
}

public sealed record AiChatSessionSummary(
    Guid Id,
    string Title,
    Guid? CourseId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AiChatMessageItem(
    Guid Id,
    string Role,
    string Content,
    IReadOnlyList<AiChatCitation> Citations,
    string? Provider,
    string? Model,
    string? PromptVersion,
    decimal? Confidence,
    bool UsedContext,
    DateTime CreatedAt);

public sealed record AiChatAnswer(
    Guid MessageId,
    string Answer,
    IReadOnlyList<AiChatCitation> Citations,
    decimal Confidence,
    bool UsedContext,
    string Provider,
    string Model,
    string PromptVersion,
    int? TokenEstimate);

public sealed record AiChatCitation(
    Guid ChunkId,
    Guid CourseId,
    Guid? SectionId,
    Guid? LessonId,
    string CourseTitle,
    string? SectionTitle,
    string? LessonTitle,
    string Snippet,
    decimal Score,
    decimal? RawScore = null);
