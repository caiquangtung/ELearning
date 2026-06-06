namespace ELearning.Application.Features.Ai.Chat;

public sealed record AiChatSessionDto(
    Guid Id,
    string Title,
    Guid? CourseId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AiChatMessageDto(
    Guid Id,
    string Role,
    string Content,
    IReadOnlyList<AiChatCitationDto> Citations,
    string? Provider,
    string? Model,
    string? PromptVersion,
    decimal? Confidence,
    bool UsedContext,
    DateTime CreatedAt);

public sealed record AiChatAnswerDto(
    Guid MessageId,
    string Answer,
    IReadOnlyList<AiChatCitationDto> Citations,
    decimal Confidence,
    bool UsedContext,
    string Provider,
    string Model);

public sealed record AiChatCitationDto(
    Guid ChunkId,
    Guid CourseId,
    Guid? SectionId,
    Guid? LessonId,
    string CourseTitle,
    string? SectionTitle,
    string? LessonTitle,
    string Snippet,
    decimal Score);
